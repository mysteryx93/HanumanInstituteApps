#!/usr/bin/env bash
set -euo pipefail

_workflows_dir=$(
  cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &&
  pwd
)

workflow_repo_root=$(
  cd -- "$_workflows_dir/../.." &&
  pwd
)

repo_root="$workflow_repo_root/source"
publish_dir="$_workflows_dir"

readonly _workflows_dir
readonly workflow_repo_root
readonly repo_root
readonly publish_dir

readonly -a SUPPORTED_APPS=(
  Player432Hz
  Converter432Hz
  PowerliminalsPlayer
  YangDownloader
)

is_supported_app() {
  local app=$1

  case "$app" in
    Player432Hz|Converter432Hz|PowerliminalsPlayer|YangDownloader)
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

require_supported_app() {
  local app=$1

  if ! is_supported_app "$app"; then
    echo "Unsupported application: $app" >&2
    echo "Supported applications: ${SUPPORTED_APPS[*]}" >&2
    exit 1
  fi
}

xml_value() {
  local key=$1
  local file=$2

  [[ -f "$file" ]] || return 0

  sed -n "s/.*<${key}>\\([^<]*\\)<\\/${key}>.*/\\1/p" "$file" | head -n 1
}

desktop_csproj() {
  local app=$1

  require_supported_app "$app"

  printf '%s\n' "$repo_root/Src/App.$app/$app.Desktop/$app.Desktop.csproj"
}

restore_desktop() {
  local app=$1
  local runtime=$2
  local proj

  proj=$(desktop_csproj "$app")

  [[ -f "$proj" ]] || {
    echo "Project not found: $proj" >&2
    exit 1
  }

  echo "Restoring $app for $runtime"
  dotnet restore "$proj" \
    -r "$runtime" \
    --verbosity minimal \
    -p:SelfContained=true \
    -p:PublishSingleFile=true \
    -p:UseAppHost=true
}

read_project_value() {
  local key=$1
  local app=$2
  local value

  value=$(xml_value "$key" "$repo_root/Src/Directory.Build.props")

  if [[ -z "$value" ]]; then
    value=$(xml_value "$key" "$(desktop_csproj "$app")")
  fi

  printf '%s\n' "$value"
}

read_shared_version() {
  local version

  version=$(xml_value AssemblyVersion "$repo_root/Src/Directory.Build.props")

  [[ -n "$version" ]] || {
    echo "AssemblyVersion is missing from Directory.Build.props." >&2
    exit 1
  }

  printf '%s\n' "$version"
}

read_version() {
  local app=$1
  local version

  version=$(read_project_value AssemblyVersion "$app")

  [[ -n "$version" ]] || {
    echo "AssemblyVersion is missing for $app." >&2
    exit 1
  }

  printf '%s\n' "$version"
}

read_product() {
  read_project_value Product "$1"
}

read_category() {
  read_project_value LSApplicationCategoryType "$1"
}

output_dir() {
  local version=$1

  printf '%s\n' "${PUBLISH_OUT:-$workflow_repo_root/$version}"
}

replace_tokens() {
  local source=$1
  local destination=$2
  shift 2

  [[ -f "$source" ]] || {
    echo "Template not found: $source" >&2
    exit 1
  }

  (( $# % 2 == 0 )) || {
    echo "replace_tokens requires token/value pairs." >&2
    exit 1
  }

  local text
  local token
  local value

  text=$(<"$source")

  while (( $# )); do
    token=$1
    value=$2
    shift 2
    text=${text//"$token"/"$value"}
  done

  printf '%s' "$text" > "$destination"
}