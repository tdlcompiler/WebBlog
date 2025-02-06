#!/bin/sh

echo "Applying database migrations..."
dotnet WebBlog.dll --migrate

echo "Starting application..."
exec dotnet WebBlog.dll
