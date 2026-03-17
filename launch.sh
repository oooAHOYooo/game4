#!/bin/bash
# Launch the Skate 4 Clone in Unity - Auto-setup & Play

PROJECT_PATH="/home/alexg/Dev/game4"

echo "🛹 Skate 4 Clone — Auto-Launch & Play"
echo "======================================"
echo "Project: $PROJECT_PATH"
echo ""
echo "→ Loading Unity..."
echo "→ Setting up Playground scene..."
echo "→ Entering Play mode..."
echo ""

# Try to find Unity executable
UNITY_CMD=""
if command -v unity &> /dev/null; then
    UNITY_CMD="unity"
elif [ -f "/home/alexg/Unity/Hub/Editor/6000.3.9f1/Editor/Unity" ]; then
    UNITY_CMD="/home/alexg/Unity/Hub/Editor/6000.3.9f1/Editor/Unity"
elif command -v /opt/unity/Editor/Unity &> /dev/null; then
    UNITY_CMD="/opt/unity/Editor/Unity"
elif [ -d "/Applications/Unity/Hub/Editors" ]; then
    # macOS
    LATEST_UNITY=$(/bin/ls -t1 /Applications/Unity/Hub/Editors | head -1)
    UNITY_CMD="/Applications/Unity/Hub/Editors/$LATEST_UNITY/Unity.app/Contents/MacOS/Unity"
else
    echo "❌ Could not find Unity. Install it or add to PATH."
    exit 1
fi

# Launch with auto-init
"$UNITY_CMD" \
    -projectPath "$PROJECT_PATH" \
    -executeMethod AutoInitialize.StartPlaytest \
    &

echo "✅ Unity launching in the background..."
echo "   (Check the Unity editor window for Play mode)"
