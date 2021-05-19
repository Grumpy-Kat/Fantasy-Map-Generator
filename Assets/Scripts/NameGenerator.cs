using System.Linq;
using UnityEngine;

public class NameGenerator : MonoBehaviour {
    // I know I forgot "Q", but it didn't make for pronounceable names
    private static string[] consonantLetters = new string[20] {
        "B", "C", "D", "F", "G", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "V", "W", "X", "Y", "Z"
    };
    private static string[] vowelLetters = new string[5] {
        "A", "E", "I", "O", "U"
    };
    private static string[] letters = consonantLetters.Concat(vowelLetters).ToArray();

    // TODO: weigh certain letters
    public static string GenerateName(System.Random random, int minLength = 4, int maxLength = 7, int maxRepeat = 2, string name = "") {
        int length = random.Next(minLength, maxLength + 1) - name.Length;
        string lastLetter = (name.Equals("") ? "" : name[name.Length - 1].ToString());
        int repeat = (name.Equals("") ? 0 : 1);
        for (int i = 0; i < length; i++) {
            string letter = letters[random.Next(0, letters.Length)];
            if (consonantLetters.Contains(letter) && consonantLetters.Contains(lastLetter)) {
                repeat++;
                if (repeat > maxRepeat) {
                    letter = vowelLetters[random.Next(0, vowelLetters.Length)];
                    repeat = 0;
                }
            } else if (vowelLetters.Contains(letter) && vowelLetters.Contains(lastLetter)) {
                repeat++;
                if (repeat > maxRepeat) {
                    letter = consonantLetters[random.Next(0, consonantLetters.Length)];
                    repeat = 0;
                }
            } else {
                repeat = 0;
            }
            name += letter;
            lastLetter = letter;
        }
        return name;
    }
}

