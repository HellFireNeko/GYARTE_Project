using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TMPro.EditorUtilities;

[CreateAssetMenu(fileName = "Input Field Validator", menuName = "Input Field Validator")]
public class IPInputValidator : TMP_InputValidator
{
    public override char Validate(ref string text, ref int pos, char ch)
    {
        Debug.Log($"ref text: {text}, ref pos: {pos}, ch: {ch}");
        pos++;
        if (char.IsNumber(ch) && text.Length < 15)
        {
            long number = long.Parse(text.Replace('.', ' ').Trim());

            if (number > 255255255255)
            {

            }
        }
        else
        {
            return '\0';
        }
    }
}
