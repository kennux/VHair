# Helper script that can be used to update UnityTK when using it as git submodule
# The unity tk code (excluding examples) will be copied to Assets/UnityTK after pulling newest updates from git
# The git submodule should be stored in UnityTK.

rm -Rf Assets/UnityTK
cd UnityTK
git pull origin master
cd ..
cp -Rf UnityTK/Assets/UnityTK Assets/UnityTK