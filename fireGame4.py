import os
import subprocess
import sys

def main():
    print("Firing up Game4 in Unity (Version 6000.3.9f1)...")

    # Detect platform and set Unity path
    if sys.platform == "win32":
        unity_path = r"C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe"
    elif sys.platform == "darwin":  # macOS
        unity_path = "/Applications/Unity/Hub/Editor/6000.3.9f1/Unity.app/Contents/MacOS/Unity"
    else:  # Linux
        unity_path = "/opt/Unity/Hub/Editor/6000.3.9f1/Editor/Unity"

    # Automatically get the parent directory of this script as the project path
    project_path = os.path.dirname(os.path.abspath(__file__))
    
    # Check if Unity executable exists
    if not os.path.exists(unity_path):
        print(f"Error: Unity executable not found at {unity_path}")
        print("Please check your Unity Hub installation paths.")
        sys.exit(1)

    # Open Unity with the specific project
    cmd = [
        unity_path,
        "-projectPath", project_path
    ]
    
    print(f"Launching Unity Editor for project: {project_path}")
    print("Get ready to play test!")
    
    # Start the process without blocking terminal 
    # Use CREATE_NEW_PROCESS_GROUP or similar if we wanted, but Popen defaults are fine to detach it
    try:
        subprocess.Popen(cmd)
    except Exception as e:
        print(f"Failed to launch Unity: {e}")

if __name__ == "__main__":
    main()
