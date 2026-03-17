#!/usr/bin/env python3
"""
🛹 Skate 4 Clone — Quick Launch Script
Finds Unity and opens the project
"""

import os
import subprocess
import sys
from pathlib import Path

def find_unity():
    """Find the Unity executable on this system."""
    # macOS
    unity_hub_path = Path("/Applications/Unity/Hub/Editor")
    if unity_hub_path.exists():
        editors = sorted(unity_hub_path.iterdir(), reverse=True)
        if editors:
            unity_app = editors[0] / "Unity.app" / "Contents" / "MacOS" / "Unity"
            if unity_app.exists():
                return str(unity_app)

    # Check if 'unity' is in PATH
    result = subprocess.run(["which", "unity"], capture_output=True)
    if result.returncode == 0:
        return "unity"

    print("❌ Could not find Unity. Install it or ensure it's in your PATH.")
    sys.exit(1)

def main():
    project_path = Path(__file__).parent.resolve()
    print(f"🛹 Skate 4 Clone — Auto-Launch & Play")
    print(f"{'='*40}")
    print(f"Project: {project_path}")
    print()
    print("→ Loading Unity...")
    print("→ Auto-populating scenes & assets...")
    print("→ Entering Play mode...")
    print()

    unity_cmd = find_unity()
    print(f"✅ Found Unity: {unity_cmd}")
    print()

    # Open in Unity editor with auto-initialization
    subprocess.Popen([
        unity_cmd,
        "-projectPath", str(project_path),
        "-executeMethod", "AutoInitialize.StartPlaytest"
    ])

    print("✅ Unity launching in the background...")
    print("   (Check the Unity editor window for Play mode)")

if __name__ == "__main__":
    main()
