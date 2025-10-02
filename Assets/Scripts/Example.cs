using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dave MovementLab - Example
///
// Content:
/// - explanations on how my code commenting is structured
///
// Note:
/// This is an example script I made to show you how I comment my codes


public class ExampleCode : MonoBehaviour
{
    /// Obvious variables that need no explanation are not being commented
    public float playerHeight;


    public float simpleVariable; // simple explanation

    
    /// more in depth explanation 
    /// this variable is so complex it needs multiple lines of explanation
    public float complexVariable;


    // explanation for multiple variables at once:
    // using only //, not ///

    public bool boolean1;
    public bool boolean2;
    public bool boolean3;
    public bool boolean4;


    /// If needed, I'll tell you above a function from where it is called
    /// For example: This function is called on the release of you right mouse button
    private void ExampleFunction()
    {
        /// again, no explanation if the code is obvious
        playerHeight += 2;

        // general explanation of what the code is doing
        complexVariable += Mathf.Lerp(complexVariable, playerHeight, 0.1f);

        /// in depth explanation on how the following code works, 
        /// why I coded it this way or what you need to pay attention to
        complexVariable = Mathf.Sin(Time.deltaTime * simpleVariable);
    }

    /// regions like this can be double clicked to open them up
    /// I usually do this if the code is not directly important for the gamePlay
    /// -> which means as a beginner just let it closed so it doesn't confuse you :D
    #region CodeHiddenInsideHere

    private void Code()
    {
        
    }

    #endregion
}
