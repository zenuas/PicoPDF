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
            var private_dict = cff.TopDict.IsCIDFont ?
                (cff.TopDict.FontDictArray[gid >= cff.TopDict.FontDictSelect.Length ? (byte)0 : cff.TopDict.FontDictSelect[gid]].PrivateDict) :
                cff.TopDict.PrivateDict;
            var local_subr = private_dict?.LocalSubroutines ?? [];

            var local_bias = Subroutine.GetSubroutineBias(local_subr.Length);

            void OperandAction(CharstringCommandCodes ope, List<float> stack, SubroutineFrame frame)
            {
                switch (ope)
                {
                    case CharstringCommandCodes.Callgsubr:
                        {
                            var index = (int)stack.Pop() + global_bias;
                            Debug.Assert(index >= 0 && index < cff.GlobalSubroutines.Length);
                            Console.WriteLine($"{ope}({index}) : {string.Join(", ", stack)}");
                            if (index >= 0 && index < cff.GlobalSubroutines.Length) Subroutine.EnumOperands(cff.GlobalSubroutines[index], stack, frame, OperandAction);
                            Console.WriteLine($"return");
                            break;
                        }

                    case CharstringCommandCodes.Callsubr:
                        {
                            var index = (int)stack.Pop() + local_bias;
                            Debug.Assert(index >= 0 && index < local_subr.Length);
                            Console.WriteLine($"{ope}({index}) : {string.Join(", ", stack)}");
                            if (index >= 0 && index < local_subr.Length) Subroutine.EnumOperands(local_subr[index], stack, frame, OperandAction);
                            Console.WriteLine($"return");
                            break;
                        }

                    case CharstringCommandCodes.Hintmask:
                    case CharstringCommandCodes.Cntrmask:
                        Console.WriteLine($"{ope} : 0b_{string.Join("_", stack.Select(x => ((int)x).ToString("b8")))}");
                        Subroutine.DefaultOperandAction(ope, stack, frame);
                        break;

                    case CharstringCommandCodes.Width:
                        frame.Width ??= stack.Count == 0 ? private_dict?.DefaultWidthX ?? 0 : (int)stack.Pop() + private_dict?.NominalWidthX ?? 0;
                        Console.WriteLine($"width : {frame.Width}");
                        break;

                    default:
                        Console.WriteLine($"{ope} : {string.Join(", ", stack)}");
                        Subroutine.DefaultOperandAction(ope, stack, frame);
                        break;
                }
            }

            Console.WriteLine($"-- {c}");
            var frame = new SubroutineFrame() { GlobalSubroutine = cff.GlobalSubroutines, LocalSubroutine = local_subr };
            Subroutine.EnumOperands(char_string, [], frame, OperandAction);
            Console.WriteLine();
        }
    }
}
