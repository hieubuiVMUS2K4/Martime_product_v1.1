#!/bin/sh
set -eu

internal_api_key="${INTERNAL_API_KEY:-}"

sed "s|__INTERNAL_API_KEY__|${internal_api_key}|g" \
  /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf