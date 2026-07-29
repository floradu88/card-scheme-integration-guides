import json
from pathlib import Path

def main() -> int:
    path = Path(__file__).parents[1] / "19-examples" / "sample-package.json"
    data = json.loads(path.read_text(encoding="utf-8"))

    errors = []
    program_ids = {p["id"] for p in data.get("programs", [])}

    for rule in data.get("rules", []):
        if rule["program_id"] not in program_ids:
            errors.append(f"Unknown program: {rule['program_id']}")
        if rule["rate"]["percentage_bps"] < 0:
            errors.append(f"Negative rate: {rule['id']}")

    if errors:
        print("\n".join(errors))
        return 1

    print(f"Valid sample package: {data['manifest']['package_id']}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
