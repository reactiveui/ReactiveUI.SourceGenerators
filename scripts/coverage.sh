#!/usr/bin/env bash
# coverage.sh - Clean build, run tests with coverage, and generate a human-readable report.
# Usage: ./scripts/coverage.sh [--project <test-project-csproj>] [--open]
#   --project <path>  Run coverage for a single test project (relative to src/)
#   --open            Open HTML report in browser after generation
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC_DIR="$REPO_ROOT/src"
RESULTS_DIR="$REPO_ROOT/test_results"
COVERAGE_DIR="$RESULTS_DIR/coverage"
REPORT_DIR="$RESULTS_DIR/report"
SOLUTION="ReactiveUI.Binding.SourceGenerators.slnx"

# Parse arguments
PROJECT=""
OPEN_REPORT=false
while [[ $# -gt 0 ]]; do
    case "$1" in
        --project)
            PROJECT="$2"
            shift 2
            ;;
        --open)
            OPEN_REPORT=true
            shift
            ;;
        *)
            echo "Unknown argument: $1" >&2
            exit 1
            ;;
    esac
done

# ── Step 1: Clean ──────────────────────────────────────────────────────────────
echo "=== Cleaning ==="
cd "$SRC_DIR"
dotnet clean "$SOLUTION" -c Release --verbosity quiet 2>/dev/null || true

echo "Removing bin/ and obj/ directories..."
find "$SRC_DIR" -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true

# Clean previous results
rm -rf "$RESULTS_DIR"
mkdir -p "$COVERAGE_DIR" "$REPORT_DIR"

# ── Step 2: Run tests with coverage ───────────────────────────────────────────
echo ""
echo "=== Building and running tests with coverage ==="
cd "$SRC_DIR"

if [[ -n "$PROJECT" ]]; then
    echo "Project: $PROJECT"
    dotnet test --project "$PROJECT" -c Release \
        --results-directory "$COVERAGE_DIR" \
        -- --coverage --coverage-output-format cobertura
else
    echo "Solution: $SOLUTION"
    dotnet test --solution "$SOLUTION" -c Release \
        --results-directory "$COVERAGE_DIR" \
        -- --coverage --coverage-output-format cobertura
fi

# ── Step 3: Find cobertura files ──────────────────────────────────────────────
echo ""
echo "=== Locating coverage files ==="

# Coverage files may land in --results-directory or in per-project TestResults/
COBERTURA_FILES=""
while IFS= read -r -d '' file; do
    COBERTURA_FILES="${COBERTURA_FILES};${file}"
done < <(find "$COVERAGE_DIR" "$SRC_DIR" -name "*.cobertura.xml" -print0 2>/dev/null)

# Strip leading semicolon
COBERTURA_FILES="${COBERTURA_FILES#;}"

if [[ -z "$COBERTURA_FILES" ]]; then
    echo "ERROR: No cobertura coverage files found." >&2
    exit 1
fi

FILE_COUNT=$(echo "$COBERTURA_FILES" | tr ';' '\n' | wc -l)
echo "Found $FILE_COUNT cobertura file(s)"

# ── Step 4: Generate reports ──────────────────────────────────────────────────
echo ""
echo "=== Generating coverage reports ==="

reportgenerator \
    -reports:"$COBERTURA_FILES" \
    -targetdir:"$REPORT_DIR" \
    -reporttypes:"Html;TextSummary;MarkdownSummaryGithub"

# ── Step 5: Display summary ──────────────────────────────────────────────────
echo ""
echo "========================================================================"
echo "                        COVERAGE SUMMARY"
echo "========================================================================"
echo ""
cat "$REPORT_DIR/Summary.txt"

# ── Step 6: Extract uncovered lines for agent consumption ─────────────────────
echo ""
echo "========================================================================"
echo "                   UNCOVERED LINES / BRANCHES"
echo "========================================================================"
echo ""

# Parse cobertura XML to find files with < 100% coverage and list uncovered lines.
# Uses xmllint (libxml2) if available, otherwise falls back to awk.
generate_uncovered_report() {
    local cobertura_file="$1"

    # Use python3 for reliable XML parsing — available on virtually all Linux systems
    python3 - "$cobertura_file" <<'PYEOF'
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict

tree = ET.parse(sys.argv[1])
root = tree.getroot()

uncovered = defaultdict(lambda: {"lines": [], "branches": []})

for package in root.iter("package"):
    for cls in package.iter("class"):
        filename = cls.get("filename", "")
        # Make path relative if possible
        for prefix in ["/home/", "/src/"]:
            idx = filename.find(prefix)
            if idx >= 0:
                filename = filename[idx:]
                break

        for line in cls.iter("line"):
            line_num = line.get("number")
            hits = int(line.get("hits", "0"))
            condition = line.get("condition-coverage", "")

            if hits == 0:
                uncovered[filename]["lines"].append(int(line_num))
            elif condition and "100%" not in condition:
                uncovered[filename]["branches"].append(
                    (int(line_num), condition)
                )

if not uncovered:
    print("All lines and branches are covered!")
    sys.exit(0)

# Sort files for stable output
for filename in sorted(uncovered.keys()):
    info = uncovered[filename]
    if not info["lines"] and not info["branches"]:
        continue
    print(f"\n--- {filename}")
    if info["lines"]:
        # Collapse consecutive lines into ranges
        lines = sorted(set(info["lines"]))
        ranges = []
        start = lines[0]
        end = lines[0]
        for n in lines[1:]:
            if n == end + 1:
                end = n
            else:
                ranges.append(f"{start}" if start == end else f"{start}-{end}")
                start = end = n
        ranges.append(f"{start}" if start == end else f"{start}-{end}")
        print(f"  Uncovered lines: {', '.join(ranges)}")
    if info["branches"]:
        for line_num, cond in sorted(set(info["branches"])):
            print(f"  Partial branch at line {line_num}: {cond}")
PYEOF
}

# Process each cobertura file
IFS=';' read -ra FILES <<< "$COBERTURA_FILES"
for f in "${FILES[@]}"; do
    if [[ -f "$f" ]]; then
        generate_uncovered_report "$f"
    fi
done

# ── Step 7: Summary footer ───────────────────────────────────────────────────
echo ""
echo "========================================================================"
echo "  HTML report: $REPORT_DIR/index.html"
echo "  Markdown:    $REPORT_DIR/SummaryGithub.md"
echo "  Text:        $REPORT_DIR/Summary.txt"
echo "========================================================================"

if $OPEN_REPORT; then
    xdg-open "$REPORT_DIR/index.html" 2>/dev/null || \
    open "$REPORT_DIR/index.html" 2>/dev/null || \
    echo "Could not open browser. Open manually: $REPORT_DIR/index.html"
fi
