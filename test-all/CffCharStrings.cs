using Mina.Command;
using Mina.Extension;
using OpenType;
using OpenType.Tables.PostScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PicoPDF.TestAll;

public class CffCharStrings : FontRegisterCommand
{
    [CommandOption("font")]
    public string Font { get; init; } = "";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font).Cast<PostScriptFont>();

        var cff = font.CompactFontFormat;
        var global_bias = Subroutine.GetSubroutineBias(cff.GlobalSubroutines.Length);

        foreach (var arg in args.Select(x => x.ToUtf32CharArray()).Flatten())
        {
            var c = char.ConvertFromUtf32(arg);
            var gid = font.CharToGID(arg);

            var char_string = cff.TopDict.CharStrings[gid < cff.TopDict.CharStrings.Length ? gid : 0];
            var fdindex = gid >= cff.TopDict.FontDictSelect.Length ? (byte)0 : cff.TopDict.FontDictSelect[gid];
            var local_subr = cff.TopDict.FontDictArray[fdindex].PrivateDict?.LocalSubroutines ?? [];
            var dict = cff.TopDict.FontDictArray[fdindex].PrivateDict;

            var local_bias = Subroutine.GetSubroutineBias(local_subr.Length);

            void OperandAction(CharstringCommandCodes ope, List<float> stack, SubroutineFrame frame)
            {
                switch (ope)
                {
                    case CharstringCommandCodes.Callgsubr:
                        {
                            var index = (int)stack.Pop() + global_bias;
                            Debug.Assert(index >= 0 && index < cff.GlobalSubroutines.Length);
                            Console.WriteLine($"ope: {ope}({index}), stack: {string.Join(", ", stack)}");
                            if (index >= 0 && index < cff.GlobalSubroutines.Length) Subroutine.EnumOperands(cff.GlobalSubroutines[index], stack, frame, OperandAction);
                            Console.WriteLine($"return");
                            break;
                        }

                    case CharstringCommandCodes.Callsubr:
                        {
                            var index = (int)stack.Pop() + local_bias;
                            Debug.Assert(index >= 0 && index < local_subr.Length);
                            Console.WriteLine($"ope: {ope}({index}), stack: {string.Join(", ", stack)}");
                            if (index >= 0 && index < local_subr.Length) Subroutine.EnumOperands(local_subr[index], stack, frame, OperandAction);
                            Console.WriteLine($"return");
                            break;
                        }

                    case CharstringCommandCodes.Hintmask:
                    case CharstringCommandCodes.Cntrmask:
                        Console.WriteLine($"ope: {ope}, stack: 0b{string.Join("_", stack.Select(x => ((int)x).ToString("b8")))}");
                        Subroutine.DefaultOperandAction(ope, stack, frame);
                        break;

                    case CharstringCommandCodes.Width:
                        Console.WriteLine($"ope: {ope}, stack: {string.Join(", ", stack)}");
                        frame.Width ??= stack.Count == 0 ? dict?.DefaultWidthX ?? 0 : (int)stack.Pop() + dict?.NominalWidthX ?? 0;
                        Console.WriteLine($"width = {frame.Width}");
                        break;

                    default:
                        Console.WriteLine($"ope: {ope}, stack: {string.Join(", ", stack)}");
                        Subroutine.DefaultOperandAction(ope, stack, frame);
                        break;
                }
            }

            Console.WriteLine($"-- {c}");
            var frame = new SubroutineFrame() { GlobalSubroutine = cff.GlobalSubroutines, LocalSubroutine = local_subr };
            Subroutine.EnumOperands(char_string, [], frame, OperandAction);
        }
    }
}
