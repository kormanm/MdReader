#!/usr/bin/env bash
# Keeps only the newest N artifacts whose name matches a pattern; deletes the rest.
# Artifact storage is shared by the whole GitHub account, so old build outputs must not pile up.
# Usage: prune-artifacts.sh '<jq regex on the artifact name>' [keep]   (needs GH_TOKEN, GITHUB_REPOSITORY)
set -euo pipefail
pattern="$1"
keep="${2:-2}"

export PATTERN="$pattern"
ids=$(gh api --paginate "repos/${GITHUB_REPOSITORY}/actions/artifacts?per_page=100" \
  --jq '.artifacts[] | select(.expired == false) | select(.name | test($ENV.PATTERN)) | [.created_at, .id] | @tsv' \
  | sort -r | tail -n +"$((keep + 1))" | cut -f2)

if [ -z "$ids" ]; then
  echo "Nothing to delete (keeping the newest $keep matching '$pattern')."
  exit 0
fi

for id in $ids; do
  echo "Deleting artifact $id"
  gh api -X DELETE "repos/${GITHUB_REPOSITORY}/actions/artifacts/${id}"
done
