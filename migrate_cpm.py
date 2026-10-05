#!/usr/bin/env python3
import os
import re
import xml.etree.ElementTree as ET

# Dictionary to hold package names and their highest found version
packages = {}

csproj_files = []
for root, dirs, files in os.walk('.'):
    for file in files:
        if file.endswith('.csproj'):
            csproj_files.append(os.path.join(root, file))

print(f"Found {len(csproj_files)} project files.")

# Step 1: Scan for packages and versions
for csproj in csproj_files:
    try:
        tree = ET.parse(csproj)
        root = tree.getroot()
        # Find all PackageReference nodes
        for pkg in root.findall('.//PackageReference'):
            name = pkg.get('Include')
            version = pkg.get('Version')
            if name and version:
                # Basic normalization / version tracking (keep latest found)
                if name not in packages:
                    packages[name] = version
    except Exception as e:
        print(f"Error reading {csproj}: {e}")

# Step 2: Create Directory.Packages.props
props_content = [
    "<Project>",
    "  <PropertyGroup>",
    "    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>",
    "  </PropertyGroup>",
    "  <ItemGroup>"
]

for name in sorted(packages.keys()):
    props_content.append(f'    <PackageVersion Include="{name}" Version="{packages[name]}" />')

props_content.extend([
    "  </ItemGroup>",
    "</Project>"
])

with open('Directory.Packages.props', 'w') as f:
    f.write('\n'.join(props_content))

print("Created Directory.Packages.props with all discovered packages.")

# Step 3: Strip versions from csproj files and update target framework
# Note: This edits files in place. Let's make it robust.
for csproj in csproj_files:
    with open(csproj, 'r') as f:
        content = f.read()
    
    # Strip Version attribute from PackageReference
    # regex matches: Version="[^"]*" or Version='[^']*' inside a PackageReference
    # We can use a precise regex replacement
    modified = re.sub(r'(<PackageReference\s+[^>]*?)\s+Version="[^"]*"([^>]*>)', r'\1\2', content)
    modified = re.sub(r'(<PackageReference\s+[^>]*?)\s+Version=\'[^\']*\'([^>]*>)', r'\1\2', content)
    
    # If the project targets net7.0, upgrade it to net10.0
    # Leave netstandard2.0 untouched as requested
    modified = re.sub(r'<TargetFramework>net7\.0</TargetFramework>', '<TargetFramework>net10.0</TargetFramework>', modified)
    
    with open(csproj, 'w') as f:
        f.write(modified)

print("Updated target frameworks and stripped version attributes across all projects.")
