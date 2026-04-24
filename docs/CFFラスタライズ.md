# CFFラスタライズ

PDFにフォントを埋め込むために必須ではないが、Compact Font Formatフォントをベクトルデータに変換しSVG形式に変換する方法を解説する。
特に断りがない場合、Type 2 Charstring Formatについて解説する。

* [https://adobe-type-tools.github.io/font-tech-notes/pdfs/5177.Type2.pdf](https://adobe-type-tools.github.io/font-tech-notes/pdfs/5177.Type2.pdf){:target="_blank"}

## CharStrings概要

Compact Font Formatの[CharStrings INDEX](CFFデータ読み込み#CharStringsINDEX)にはグリフを描画するPostScriptバイナリが格納されている。
PostScriptは一部例外[^PolishNotation]を除いて[逆ポーランド記法](https://en.wikipedia.org/wiki/Reverse_Polish_notation){:target="_blank"}で記述されている。

[^PolishNotation]: 前置きになる命令はhintmask、cntrmask、shortint

### 可変長数値

前置きの可変長で表現される数値である。
b0が32～255の場合は可変長数値となる。
先頭からb0、b1、b2、b3、b4と並んでいる場合の計算方法である。

| サイズ | b0の範囲   | 値の範囲                   | 計算式                                              |
|-------:|------------|----------------------------|-----------------------------------------------------|
|      1 | 32 ～ 246  | -107 ～ +107               | b0 - 139                                            |
|      2 | 247 ～ 250 | +108 ～ +1131              | (b0 - 247) * 256 + b1 + 108                         |
|      2 | 251 ～ 254 | -1131 ～ -108              | -(b0 - 251) * 256 - b1 - 108                        |
|      5 | 255        | -(2 ^ 31) ～ +(2 ^ 31 - 1) | (b1 << 8 &#124; b2) + ((b3 << 8 &#124; b4) / 65536) |

[DICT Dataのnumber](CFFデータ読み込み#DICTDataのnumberについて)とほぼ同じだが、b0が255の場合の挙動が異なる。
また、b0が28の場合は[shortint](#shortint)命令と定義されているが、挙動はDICTDataのnumberと同じである。

```cs
var b0 = CharStrings[i];

if (b0 is >= 32 and <= 246) number = b0 - 139;
else if (b0 is >= 247 and <= 250) number = (b0 - 247) * 256 + CharStrings[++i] + 108;
else if (b0 is >= 251 and <= 254) number = -((b0 - 251) * 256) - CharStrings[++i] - 108;
else if (b0 == 255) number = ((short)(CharStrings[++i] << 8 | CharStrings[++i])) + ((short)(CharStrings[++i] << 8 | CharStrings[++i])) / 65536f;
```

以降の例では可変長数値は変換されているものとして説明する。

### 命令

命令(オペレーター)は1～2バイトの後置き[^PolishNotation]の可変長となる。
b0が31以下であれば命令である。
b0が12の場合はエスケープとし、次の1バイトと合わせて2バイト命令となる。
スタックを引数として命令を実行する。
定義が `|-` で始まる場合、スタックの底から全て引数として消費し、スタックを空にしてから次の命令実行に移ることを意味する。
`x`、`y` は絶対座標、`dx`、`dy` で始まるのは相対座標である。

| 値         | 命令                                             | 定義                                                                                                                                                   |
|------------|--------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| 0          | 予約                                             |                                                                                                                                                        |
| 1          | [hstem](#hstem、vstem、hstemhm、vstemhm)         | &#124;- y dy {dya dyb}* hstem &#124;-                                                                                                                  |
| 2          | 予約                                             |                                                                                                                                                        |
| 3          | [vstem](#hstem、vstem、hstemhm、vstemhm)         | &#124;- x dx {dxa dxb}* vstem &#124;-                                                                                                                  |
| 4          | [vmoveto](#rmoveto、vmoveto、hmoveto)            | &#124;- dy1 vmoveto &#124;-                                                                                                                            |
| 5          | [rlineto](#rlineto、vlineto、hlineto)            | &#124;- {dxa dya}+ rlineto &#124;-                                                                                                                     |
| 6          | [hlineto](#rlineto、vlineto、hlineto)            | &#124;- dx1 {dya dxb}* hlineto &#124;-<br>&#124;- {dxa dyb}+ hlineto &#124;-                                                                           |
| 7          | [vlineto](#rlineto、vlineto、hlineto)            | &#124;- dy1 {dxa dyb}* vlineto &#124;-<br>&#124;- {dya dxb}+ vlineto &#124;-                                                                           |
| 8          | [rrcurveto](#rrcurveto、rcurveline、rlinecurve)  | &#124;- {dxa dya dxb dyb dxc dyc}+ rrcurveto &#124;-                                                                                                   |
| 9          | 予約                                             |                                                                                                                                                        |
| 10         | [callsubr](#callsubr、callgsubr)                 | subr callsubr                                                                                                                                          |
| 11         | [return](#return)                                | return                                                                                                                                                 |
| 12         | [escape](#escape)                                | 次の1バイトと合わせて2バイト命令とする                                                                                                                 |
| 13         | 予約                                             |                                                                                                                                                        |
| 14         | [endchar](#endchar)                              | endchar                                                                                                                                                |
| 15         | 予約                                             |                                                                                                                                                        |
| 16         | 予約                                             |                                                                                                                                                        |
| 17         | 予約                                             |                                                                                                                                                        |
| 18         | [hstemhm](#hstem、vstem、hstemhm、vstemhm)       | &#124;- y dy {dya dyb}* hstemhm &#124;-                                                                                                                |
| 19         | [hintmask](#hintmask、cntrmask)                  | &#124;- (x dx {dxa dxb}*)? hintmask mask+ &#124;-                                                                                                      |
| 20         | [cntrmask](#hintmask、cntrmask)                  | &#124;- (x dx {dxa dxb}*)? cntrmask mask+ &#124;-                                                                                                      |
| 21         | [rmoveto](#rmoveto、vmoveto、hmoveto)            | &#124;- dx1 dy1 rmoveto &#124;-                                                                                                                        |
| 22         | [hmoveto](#rmoveto、vmoveto、hmoveto)            | &#124;- dx1 hmoveto &#124;-                                                                                                                            |
| 23         | [vstemhm](#hstem、vstem、hstemhm、vstemhm)       | &#124;- x dx {dxa dxb}* vstemhm &#124;-                                                                                                                |
| 24         | [rcurveline](#rrcurveto、rcurveline、rlinecurve) | &#124;- {dxa dya dxb dyb dxc dyc}+ dxd dyd rcurveline &#124;-                                                                                          |
| 25         | [rlinecurve](#rrcurveto、rcurveline、rlinecurve) | &#124;- {dxa dya}+ dxb dyb dxc dyc dxd dyd rlinecurve &#124;-                                                                                          |
| 26         | [vvcurveto](#vvcurveto、hhcurveto)               | &#124;- dx1? {dya dxb dyb dyc}+ vvcurveto &#124;-                                                                                                      |
| 27         | [hhcurveto](#vvcurveto、hhcurveto)               | &#124;- dy1? {dxa dxb dyb dxc}+ hhcurveto &#124;-                                                                                                      |
| 28         | [shortint](#shortint)                            | shortint b1 b2                                                                                                                                         |
| 29         | [callgsubr](#callsubr、callgsubr)                | globalsubr callgsubr                                                                                                                                   |
| 30         | [vhcurveto](#vhcurveto、hvcurveto)               | &#124;- dy1 dx2 dy2 dx3 {dxa dxb dyb dyc dyd dxe dye dxf}* dyf? vhcurveto &#124;-<br>&#124;- {dya dxb dyb dxc dxd dxe dye dyf}+ dxf? vhcurveto &#124;- |
| 31         | [hvcurveto](#vhcurveto、hvcurveto)               | &#124;- dx1 dx2 dy2 dy3 {dya dxb dyb dxc dxd dxe dye dyf}* dxf? hvcurveto &#124;-<br>&#124;- {dxa dxb dyb dyc dyd dxe dye dxf}+ dyf? hvcurveto &#124;- |
| 12 0       | 予約                                             |                                                                                                                                                        |
| 12 1       | 予約                                             |                                                                                                                                                        |
| 12 2       | 予約                                             |                                                                                                                                                        |
| 12 3       | [and](#and、or、eq)                              | num1 num2 and                                                                                                                                          |
| 12 4       | [or](#and、or、eq)                               | num1 num2 or                                                                                                                                           |
| 12 5       | [not](#not、neg)                                 | num1 not                                                                                                                                               |
| 12 6       | 予約                                             |                                                                                                                                                        |
| 12 7       | 予約                                             |                                                                                                                                                        |
| 12 8       | 予約                                             |                                                                                                                                                        |
| 12 9       | [abs](#abs)                                      | num abs                                                                                                                                                |
| 12 10      | [add](#add、sub、mul、div)                       | num1 num2 add                                                                                                                                          |
| 12 11      | [sub](#add、sub、mul、div)                       | num1 num2 sub                                                                                                                                          |
| 12 12      | [div](#add、sub、mul、div)                       | num1 num2 div                                                                                                                                          |
| 12 13      | 予約                                             |                                                                                                                                                        |
| 12 14      | [neg](#not、neg)                                 | num neg                                                                                                                                                |
| 12 15      | [eq](#and、or、eq)                               | num1 num2 eq                                                                                                                                           |
| 12 16      | 予約                                             |                                                                                                                                                        |
| 12 17      | 予約                                             |                                                                                                                                                        |
| 12 18      | [drop](#drop)                                    | num drop                                                                                                                                               |
| 12 19      | 予約                                             |                                                                                                                                                        |
| 12 20      | [put](#put、get)                                 | val i put                                                                                                                                              |
| 12 21      | [get](#put、get)                                 | i get                                                                                                                                                  |
| 12 22      | [ifelse](#ifelse)                                | s1 s2 v1 v2 ifelse                                                                                                                                     |
| 12 23      | [random](#random)                                | random                                                                                                                                                 |
| 12 24      | [mul](#add、sub、mul、div)                       | num1 num2 mul                                                                                                                                          |
| 12 25      | 予約                                             |                                                                                                                                                        |
| 12 26      | [sqrt](#sqrt)                                    | num sqrt                                                                                                                                               |
| 12 27      | [dup](#dup)                                      | any dup                                                                                                                                                |
| 12 28      | [exch](#exch)                                    | num1 num2 exch                                                                                                                                         |
| 12 29      | [index](#index)                                  | i index                                                                                                                                                |
| 12 30      | [roll](#roll)                                    | N J roll                                                                                                                                               |
| 12 31      | 予約                                             |                                                                                                                                                        |
| 12 32      | 予約                                             |                                                                                                                                                        |
| 12 33      | 予約                                             |                                                                                                                                                        |
| 12 34      | [hflex](#hflex)                                  | &#124;- dx1 dx2 dy2 dx3 dx4 dx5 dx6 hflex &#124;-                                                                                                      |
| 12 35      | [flex](#flex)                                    | &#124;- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 dx6 dy6 fd flex &#124;-                                                                                |
| 12 36      | [hflex1](#hflex1)                                | &#124;- dx1 dy1 dx2 dy2 dx3 dx4 dx5 dy5 dx6 hflex1 &#124;-                                                                                             |
| 12 37      | [flex1](#flex1)                                  | &#124;- dx1 dy1 dx2 dy2 dx3 dy3 dx4 dy4 dx5 dy5 d6 flex1 &#124;-                                                                                       |
| 12 38～255 | 予約                                             |                                                                                                                                                        |

### width

PostScriptバイナリには命令の並びが定められている。
並び順は `width? ステム系命令* ヒント系命令* {moveto系命令 サブパス命令}* endchar` である。
先頭のwidthはグリフの送り幅である。
サブパス命令[^Subpath]以外のhstem、vstem、hstemhm、vstemhm、hintmask、cntrmask、rmoveto、vmoveto、hmoveto、endchar命令がwidthの直後になりうる。
命令実行時のスタックの残りがwidthとなる。
これらはスタックの消費数が固定のため、余った先頭の数値がwidthとなる。

[^Subpath]: サブパス命令はrlineto、vlineto、hlineto、rrcurveto、rcurveline、rlinecurve、vvcurveto、hhcurveto、vhcurveto、hvcurveto

| 命令     | スタック消費数 | 補足                                                                                   |
|----------|----------------|----------------------------------------------------------------------------------------|
| hstem    | 偶数           | スタック数が奇数であればスタックの底をwidthとする                                      |
| vstem    | 偶数           | スタック数が奇数であればスタックの底をwidthとする                                      |
| hstemhm  | 偶数           | スタック数が奇数であればスタックの底をwidthとする                                      |
| vstemhm  | 偶数           | スタック数が奇数であればスタックの底をwidthとする                                      |
| hintmask | 偶数           | スタック数が奇数であればスタックの底をwidthとする、残りのスタックはvstemのヒントとなる |
| cntrmask | 偶数           | スタック数が奇数であればスタックの底をwidthとする、残りのスタックはvstemのヒントとなる |
| rmoveto  | 2個            | スタック数が3であればスタックの底をwidthとする                                         |
| vmoveto  | 1個            | スタック数が2であればスタックの底をwidthとする                                         |
| hmoveto  | 1個            | スタック数が2であればスタックの底をwidthとする                                         |
| endchar  | 0個            | スタック数が1であればスタックの底をwidthとする                                         |

widthは[Private DICT](CFFデータ読み込み#PrivateDICT)のnominalWidthXを加算したものが実際のwidthとなる。
widthが指定されなかった場合はdefaultWidthXがwidthとなる。

```cs
switch (ope)
{
	case Hstem:
	case Vstem:
	case Hstemhm:
	case Vstemhm:
	case Hintmask:
	case Cntrmask:
		if (stack.Count % 2 == 1) width ?= stack.Shift() + nominalWidthX;
		break;
	
	case Rmoveto:
		if (stack.Count > 2) width ?= stack.Shift() + nominalWidthX;
		break;
	
	case Vmoveto:
	case Hmoveto:
		if (stack.Count > 1) width ?= stack.Shift() + nominalWidthX;
		break;
	
	case Endchar:
		if (stack.Count > 0) width ?= stack.Shift() + nominalWidthX;
		break;
}
width ?= defaultWidthX; // widthが未指定の場合
```

以降の例ではwidthはスタックから取り除かれたものとして説明する。

### パスの終了

サブパス命令[^Subpath]で面を構成する。
面を構成するパスは閉じている必要があるため、最後の点から最初の点につなぐ必要がある。
moveto系命令[^Moveto]や[endchar](#endchar)が出現するとパスを閉じる必要がある。
SVGであれば[パスを閉じるコマンド](https://developer.mozilla.org/en-US/docs/Web/SVG/Reference/Attribute/d#closepath){:target="_blank"}がある。

[^Moveto]: moveto系命令はrmoveto、vmoveto、hmoveto

### rmoveto、vmoveto、hmoveto

現在座標を (dx1, dy1) に移動する。座標は相対指定である。
最初の座標は (0, 0) である。

### rlineto、vlineto、hlineto

rlinetoは現在位置から相対座標で (dxa, dya) に直線を描画する。
vlinetoはdy1の縦直線を描画した後dxaの横直線→dybの縦直線を描画する。
hlinetoはdx1の横直線を描画した後dyaの縦直線→dxbの横直線を描画する。

```
50, 250 Rmoveto        # 現在座標を(50, 250)へ移動
250, 70, -250 hlineto  # (50, 250)-(50, 500)の横線 → (50, 500)-(120, 500)の縦線 → (120, 500)-(120, 250)の横線を描画
endchar                # パスを閉じるため(120, 250)-(50, 250)の直線を描画してアウトライン終了
```

### rrcurveto、rcurveline、rlinecurve

rrcurvetoとrcurvelineは現在位置から相対座標で (dxa, dya) を1番目の制御点、(dxb, dyb) を2番目の制御点とし (dxc, dyc) への3次ベジェ曲線を描画する。
その後、rcurvelineは (dxd, dyd) に直線を描画する。

rlinecurveは現在位置から相対座標で (dxa, dya) に直線を引いた後、 (dxb, dyb) を1番目の制御点、(dxc, dyc) を2番目の制御点とし (dxd, dyd) への3次ベジェ曲線を描画する。

```cs
if (ope == Rlinecurve)
{
	while (stack.Count >= 8)
	{
		var end = new Vector2(start.X + stack.Shift(), start.Y + stack.Shift());
		// start-end の直線を描画
		start = end;
	}
}
while (stack.Count >= 6)
{
	var cp1 = new Vector2(start.X + stack.Shift(), start.Y + stack.Shift());
	var cp2 = new Vector2(cp1.X + stack.Shift(), cp1.Y + stack.Shift());
	var end = new Vector2(cp2.X + stack.Shift(), cp2.Y + stack.Shift());
	// start-cp1-cp2-end のベジェ曲線を描画
	start = end;
}
if (ope == Rcurveline)
{
	var end = new Vector2(start.X + stack.Shift(), start.Y + stack.Shift());
	// start-end の直線を描画
	start = end;
}
```

### vvcurveto、hhcurveto

現在位置から相対座標で (dx1, dy1) に移動したのち、相対座標で (dxa, dya) を1番目の制御点、(dxb, dyb) を2番目の制御点とし (dxc, dyc) への3次ベジェ曲線を描画する。

```cs
if (stack.Count % 2 != 0) start = new(start.X + ope == Vvcurveto ? stack.Shift() : 0, start.Y + ope != Vvcurveto ? stack.Shift() : 0);
while (stack.Count >= 4)
{
	var cp1 = new Vector2(start.X + ope == Vvcurveto ? stack.Shift() : 0, start.Y + ope != Vvcurveto ? stack.Shift() : 0);
	var cp2 = new Vector2(cp1.X + stack.Shift(), cp1.Y + stack.Shift());
	var end = new Vector2(cp2.X + ope != Vvcurveto ? stack.Shift() : 0, cp2.Y + ope == Vvcurveto ? stack.Shift() : 0);
	// start-cp1-cp2-end のベジェ曲線を描画
	start = end;
}
```

### vhcurveto、hvcurveto

### shortint

前置き型の命令である。
次の2バイトをshort型変数としてスタックに入れる。
[可変長数値](#可変長数値)の一部として扱ってもよい。

```cs
var b0 = CharStrings[i];

if (b0 == Shortint) number = (short)(CharStrings[++i] << 8 | CharStrings[++i]);
```

### callsubr、callgsubr

### return

サブルーチンを終了する。
スタックに積まれた数値は戻り値となる。

### endchar

アウトラインを終了する。
サブルーチン内でendcharが登場する可能性もある。

### escape

### hstem、vstem、hstemhm、vstemhm

### hintmask、cntrmask

### add、sub、mul、div

### and、or、eq

### not、neg

### abs

### sqrt

### drop

### put、get

### ifelse

### random

### exch

### index

### roll

### hflex

### flex

### hflex1

### flex1

## 実装の優先順位

通常のフォントでは使用されない命令も多い。
[可変長数値](#可変長数値)、shortint、callsubr、callgsubr、return、endcharは実装必須である。
通常のフォントでは上記に加えmoveto系命令[^Moveto]、サブパス命令[^Subpath]だけを実装すれば十分な場合が多い。

ただし、通常のフォントにはステム系[^Stem]、hintmask、cntrmaskが含まれるため無視することができない。
ヒントはフォントを小さなサイズで表示する場合の情報である。
SVG形式に変換する場合は無視してしまって構わない。

[^Stem]: ステム系命令はhstem、vstem、hstemhm、vstemhm

```cs
switch (ope)
{
	case Hstem:
	case Vstem:
	case Hstemhm:
	case Vstemhm:
		stem += stack.Count / 2; // ヒントは偶数個ペア
		stack.Clear();
		break;
	
	case Hintmask:
	case Cntrmask:
		stem += stack.Count / 2; // スタックがあればvstem相当
		i += (stem + 7) / 8; // ヒントのペア数 * 1bitのマスクを読み飛ばす
		stack.Clear();
		break;
}
```
