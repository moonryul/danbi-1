[System.Serializable]
public enum EDanbiUndistortMode : int {        // Moon changed this enum to include "NoUndistort" mode (negative enum)
  E_NoUndistort = -1,
  E_Iterative = 0,
  E_Direct = 1,
  E_Newton = 2,
};