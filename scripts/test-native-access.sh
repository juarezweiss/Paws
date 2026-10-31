#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ACCESS="$ROOT/UnityProject/Assets/CatsAndArrows/Access/Plugins/iOS"
RUN="$(mktemp -d "${TMPDIR:-/tmp}/paws-native-access.XXXXXX")"
trap 'rm -rf "$RUN"' EXIT
xcrun swiftc -module-cache-path "$RUN/cache" "$ACCESS/PawsAccessWorkflow.swift" \
  "$ROOT/scripts/tests/native_access_tests.swift" -o "$RUN/workflow-tests"
"$RUN/workflow-tests"
SDK="$(xcrun --sdk iphoneos --show-sdk-path)"
for target in arm64-apple-ios16.3 arm64-apple-ios26.0 arm64-apple-ios26.2 arm64-apple-ios26.4 arm64-apple-ios27.0; do
  xcrun swiftc -typecheck -sdk "$SDK" -target "$target" -module-cache-path "$RUN/cache-ios" \
    "$ACCESS/PawsAccessWorkflow.swift" "$ACCESS/PawsNativeAccess.swift"
  echo "Native access SDK typecheck: $target passed"
done
