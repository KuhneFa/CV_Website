#!/bin/sh
set -e

mkdir -p /data/dp-keys 2>/dev/null || true
chown -R cvapp:cvapp /data /app 2>/dev/null || true

if command -v runuser >/dev/null 2>&1; then
    exec runuser -u cvapp -- dotnet CV_Website.dll
fi

if command -v su >/dev/null 2>&1; then
    exec su cvapp -s /bin/sh -c "dotnet CV_Website.dll"
fi

exec dotnet CV_Website.dll
