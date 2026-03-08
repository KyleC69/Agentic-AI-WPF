// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ContextWindowContinuityTests.cs
//   Author: Kyle L. Crowder



public static class ContextWindowContinuityTests
{


    // --- Phase 3: Reasoning continuity tests (requires earlier outputs) ---

    public const string ReasoningFromD =
            "Reasoning Test 1: Take the prime number you gave in Task D and add 5 to it.";

    public const string ReasoningFromE =
            "Reasoning Test 2: Take the result from Task E and multiply it by 3.";

    public const string ReasoningFromI =
            "Reasoning Test 3: Using the sunrise sentence from Task I, rewrite it in past tense.";

    public const string RecallFromA =
            "Recall Test 5: In Task A, what was the reversed string you produced?";


    // --- Phase 2: Context recall tests referencing earlier tasks ---

    public const string RecallFromC =
            "Recall Test 1: In Task C, you produced a 5-word sentence. Repeat that exact sentence.";

    public const string RecallFromF =
            "Recall Test 2: In Task F, you listed three fruits. Repeat the same three fruits in the same order.";

    public const string RecallFromH =
            "Recall Test 3: In Task H, you gave me a color starting with 'M'. What was it?";

    public const string RecallFromJ =
            "Recall Test 4: In Task J, what number did you output?";


    // --- Phase 4: Long-range stress tests ---

    public const string StressDistraction =
            "Provide a detailed explanation of how hydraulic brakes work in a vehicle.";

    public const string StressRecallFromB =
            "After that long explanation, return to Task B. How many vowels did you count in 'consequence'?";

    public const string StressRecallFromG =
            "Also, what adjective did you give me in Task G?";
    // --- Phase 1: Fill the context with unrelated tasks (A–J) ---

    public const string TaskA =
            "Task A: Reverse the string 'lantern'. Do not explain, just output the reversed string.";

    public const string TaskB =
            "Task B: Count the number of vowels in the word 'consequence'.";

    public const string TaskC =
            "Task C: Provide a 5-word sentence about a robot learning to dance.";

    public const string TaskD =
            "Task D: Give me a random 2-digit prime number.";

    public const string TaskE =
            "Task E: Convert the number 144 into its square root.";

    public const string TaskF =
            "Task F: Output a three-item list of fruits in alphabetical order.";

    public const string TaskG =
            "Task G: Provide a single adjective that describes a quiet forest.";

    public const string TaskH =
            "Task H: Give me a color that starts with the letter 'M'.";

    public const string TaskI =
            "Task I: Provide a one-sentence description of a sunrise.";

    public const string TaskJ =
            "Task J: Output the number of letters in the word 'framework'.";








    public static List<string> GetList()
    {
        return
        [
                TaskA, TaskB, TaskC, TaskD, TaskE, TaskF, TaskG, TaskH, TaskI, TaskJ,
                RecallFromC, RecallFromF, RecallFromH, RecallFromJ, RecallFromA,
                ReasoningFromD, ReasoningFromE, ReasoningFromI,
                StressDistraction, StressRecallFromB, StressRecallFromG
        ];
    }
}