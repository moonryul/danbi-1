[System.Serializable]
public enum EDanbiUndistortMode : int {        // Moon changed this enum to include "NoUndistort" mode (negative enum)
    E_Direct = 0,
    E_Iterative = 1,     
    E_Newton = 2,
};