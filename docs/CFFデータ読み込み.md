# CFFデータ読み込み

CFFテーブルにはCompact Font Formatのファイルが入っている。
OpenTypeの他テーブルとは異なり、Compact Font Formatという全く別のデータ構造が入っているものと考えて差し支えない。
特に断りがない場合、Compact Font Formatバージョン1について解説する。

* [https://adobe-type-tools.github.io/font-tech-notes/pdfs/5176.CFF.pdf](https://adobe-type-tools.github.io/font-tech-notes/pdfs/5176.CFF.pdf){:target="_blank"}

CFFテーブルは以下の5つのデータ構造で構成されている。
Charsetsまでのデータ構造の出現順は固定である。

| データ構造        | 補足                                             |
|-------------------|--------------------------------------------------|
| Header            |                                                  |
| Name INDEX        | INDEX Data形式                                   |
| Top DICT INDEX    | INDEX Data形式の中にDICT Dataが格納              |
| String INDEX      | INDEX Data形式                                   |
| Global Subr INDEX | INDEX Data形式                                   |
| Encodings         | Top DICTでオフセット指定、OpenTypeでは存在しない |
| Charsets          | Top DICTでオフセット指定                         |
| Private DICT      | Top DICTでオフセット指定                         |
| Local Subr INDEX  | Top DICTでオフセット指定                         |
| CharStrings INDEX | Top DICTでオフセット指定                         |
| FDArray           | Top DICTでオフセット指定、CIDフォントのみ格納    |
| FDSelect          | Top DICTでオフセット指定、CIDフォントのみ格納    |

## INDEX Data

Header以外はINDEX Dataという形式で格納されている。

| タイプ | 名前              | 補足                                      |
|--------|-------------------|-------------------------------------------|
| ushort | count             | INDEXの個数、ゼロの場合は後続のデータなし |
| byte   | offSize           | オフセットの要素サイズ、1～4              |
| 可変   | offset[count + 1] | オブジェクトデータのサイズ                |
| byte   | data[count][可変] | オブジェクトデータ                        |

INDEX Dataの読み込みは次のようになる。

```cs
var count = (ushort)Read2Byte();
if (count == 0) return;

var offSize = Read1Byte();
var offset = new int[count + 1];
for (var i = 0; i < count + 1; i++) offset = (int)ReadBytes(offSize);

var data = new byte[count][];
for (var i = 0; i < count; i++) data[i] = ReadBytes(offset[i + 1] - offset[i]);
```

## DICT Data

DICT Dataはキーバリュー形式の辞書データである。
データ長は辞書データ中になく、外部から与えられる。(Top DICTであればINDEX Dataの長さ、Private DICTであればDICT Dataのサイズとオフセットが格納)
キーとデータ長はあらかじめ決められたもののみである。

キーは[Top DICT](#TopDICTINDEX)と[Private DICT](#PrivateDICT)で格納されるデータが異なるため、各項で説明する。

### DICT Dataのnumberについて

numberは可変長で表現される数値である。
b0が28～254(30を除く)の場合は可変長整数となる。
先頭からb0、b1、b2、b3、b4と並んでいる場合の計算方法である。

| サイズ | b0の範囲   | 値の範囲                   | 計算式                                            |
|-------:|------------|----------------------------|---------------------------------------------------|
|      1 | 32 ～ 246  | -107 ～ +107               | b0 - 139                                          |
|      2 | 247 ～ 250 | +108 ～ +1131              | (b0 - 247) * 256 + b1 + 108                       |
|      2 | 251 ～ 254 | -1131 ～ -108              | -(b0 - 251) * 256 - b1 - 108                      |
|      3 | 28         | -32768 ～ +32767           | b1 << 8 &#124; b2                                 |
|      5 | 29         | -(2 ^ 31) ～ +(2 ^ 31 - 1) | b1 << 24 &#124; b2 << 16 &#124; b3 << 8 &#124; b4 |

b0が30の場合は後続のバイト列が4bitで1桁を表す実数となる。
[パック10進数(Packed BCD)](https://en.wikipedia.org/wiki/Binary-coded_decimal)の亜種である。

| 4bit     | 意味         |
|----------|--------------|
| 0 ～ 9   | 0 ～ 9の数値 |
| 10 (0xA) | 小数点       |
| 11 (0xB) | E            |
| 12 (0xC) | E-           |
| 13 (0xD) | 予約         |
| 14 (0xE) | マイナス     |
| 15 (0xF) | 終端         |

## Header

バージョン情報などが格納されている。
Compact Font Formatバージョン1では4バイト固定で読み進めればよい。

| タイプ | 名前    | 補足               |
|--------|---------|--------------------|
| byte   | major   | メジャーバージョン |
| byte   | minor   | マイナーバージョン |
| byte   | hdrSize | Headerサイズ       |
| byte   | offSize | オフセットサイズ   |

## Name INDEX

INDEX Data形式でフォント名が格納されている。
OpenTypeではnameテーブルを参照するため使用する必要はない。

## Top DICT INDEX

INDEX Data形式で読み込んだデータがDICT Data形式になっている。
Top DICTにROS(12, 30)が存在すればCIDフォントになる。
Top DICTに格納された、charset、CharStrings、Private、FDArray、FDSelectを読む必要がある。
SIDはString INDEXのインデックスである。

| 名前               | キー   | バリュータイプ | 初期値、補足                      |
|--------------------|--------|----------------|-----------------------------------|
| version            | 0      | SID            |                                   |
| Notice             | 1      | SID            |                                   |
| Copyright          | 12, 0  | SID            |                                   |
| FullName           | 2      | SID            |                                   |
| FamilyName         | 3      | SID            |                                   |
| Weight             | 4      | SID            |                                   |
| isFixedPitch       | 12, 1  | boolean        | false                             |
| ItalicAngle        | 12, 2  | number         | 0                                 |
| UnderlinePosition  | 12, 3  | number         | -100                              |
| UnderlineThickness | 12, 4  | number         | 50                                |
| PaintType          | 12, 5  | number         | 0                                 |
| CharstringType     | 12, 6  | number         | 2                                 |
| FontMatrix         | 12, 7  | array          | 0.001, 0, 0, 0.001, 0, 0          |
| UniqueID           | 13     | number         |                                   |
| FontBBox           | 5      | array          | 0, 0, 0, 0                        |
| StrokeWidth        | 12, 8  | number         | 0                                 |
| XUID               | 14     | array          |                                   |
| charset            | 15     | number         | 0 charsetデータのオフセット       |
| Encoding           | 16     | number         | 0 Encodingデータのオフセット      |
| CharStrings        | 17     | number         | CharStringsデータのオフセット     |
| Private            | 18     | number, number | Privateデータのサイズとオフセット |
| SyntheticBase      | 12, 20 | number         |                                   |
| PostScript         | 12, 21 | SID            |                                   |
| BaseFontName       | 12, 22 | SID            |                                   |
| BaseFontBlend      | 12, 23 | delta          |                                   |

CIDフォントでは以下のデータが含まれる。

| 名前            | キー   | バリュータイプ   | 初期値、補足               |
|-----------------|--------|------------------|----------------------------|
| ROS             | 12, 30 | SID, SID, number | CIDフォント                |
| CIDFontVersion  | 12, 31 | number           | 0                          |
| CIDFontRevision | 12, 32 | number           | 0                          |
| CIDFontType     | 12, 33 | number           | 0                          |
| CIDCount        | 12, 34 | number           | 8720                       |
| UIDBase         | 12, 35 | number           |                            |
| FDArray         | 12, 36 | number           | FDArrayデータのオフセット  |
| FDSelect        | 12, 37 | number           | FDSelectデータのオフセット |
| FontName        | 12, 38 | SID              |                            |

なお、オフセットは全てCompact Font Formatの先頭からのオフセットである。

## String INDEX

INDEX Data形式で文字列が格納されている。
SIDというインデックスで参照される。
SIDのうち0～390までは定義済みの文字列になる。(定義値はThe Compact Font Format SpecificationのAppendix A Standard Strings参照)

String INDEXで格納された文字列は391番以降で参照されることになる。

* [https://github.com/zenuas/PicoPDF/blob/master/font/Tables/PostScript/SID.cs](https://github.com/zenuas/PicoPDF/blob/master/font/Tables/PostScript/SID.cs)

## Global Subr INDEX

INDEX Data形式でCharStringsが格納されている。
後述する[CharStrings](#CharStrings)から参照されるグローバル関数である。

## Charsets

Compact Font FormatではグリフデータをGIDではなくSIDまたはCIDで参照する。
CharsetsはGIDからSIDまたはCIDを取得するためのデータになる。
Top Dictのcharsetデータのオフセットによって参照される。

Charsetsのオフセットが0～2の時は定義済みのcharsetが使用される。

| オフセット | 名前         |
|------------|--------------|
| 0          | ISOAdobe     |
| 1          | Expert       |
| 2          | ExpertSubset |

上記でない場合はオフセットから1バイト読み込みフォーマットを決定する。
CharsetsではGID 0(.notdef)のデータは格納されない。

Format0:

| タイプ | 名前               | 説明                                |
|--------|--------------------|-------------------------------------|
| byte   | format             | 0固定                               |
| SID    | glyph[nGlyphs - 1] | nGlyphsはCharStrings INDEXのcount値 |

Format1または2:

| タイプ | 名前        | 説明   |
|--------|-------------|--------|
| byte   | format      | 1 or 2 |
| struct | Range[可変] |        |

Range:

| タイプ         | 名前  | 説明                               |
|----------------|-------|------------------------------------|
| SID            | first | 先頭のSID                          |
| byte or ushort | nLeft | Format1ならbyte、Format2ならushort |

Charsetsの読み込みは次のようになる。

```cs
var format = Read1Byte();
var SID = new ushort[nGlyphs - 1];

if (format == 0)
{
	for (var i = 0; i < nGlyphs - 1; i++) SID[i] = (ushort)Read2Byte();
}
else
{
	var count = 0;
	while (count < nGlyphs - 1)
	{
		var first = (ushort)Read2Byte();
		var nLeft = format == 1 ? Read1Byte() : (ushort)Read2Byte();
		for (var i = 0; i < nLeft + 1; i++) SID[count++] = first + i;
	}
}
```

## Private DICT

Top DICTのPrivateやFDArrayからオフセット指定され、DICT Data形式になっている。

| 名前              | キー   | バリュータイプ | 初期値、補足                       |
|-------------------|--------|----------------|------------------------------------|
| BlueValues        | 6      | delta          |                                    |
| OtherBlues        | 7      | delta          |                                    |
| FamilyBlues       | 8      | delta          |                                    |
| FamilyOtherBlues  | 9      | delta          |                                    |
| BlueScale         | 12, 9  | number         | 0.039625                           |
| BlueShift         | 12, 10 | number         | 7                                  |
| BlueFuzz          | 12, 11 | number         | 1                                  |
| StdHW             | 10     | number         |                                    |
| StdVW             | 11     | number         |                                    |
| StemSnapH         | 12, 12 | delta          |                                    |
| StemSnapV         | 12, 13 | delta          |                                    |
| ForceBold         | 12, 14 | boolean        | false                              |
| LanguageGroup     | 12, 17 | number         | 0                                  |
| ExpansionFactor   | 12, 18 | number         | 0.06                               |
| initialRandomSeed | 12, 19 | number         | 0                                  |
| Subrs             | 19     | number         | Local Subr INDEXデータのオフセット |
| defaultWidthX     | 20     | number         | 0                                  |
| nominalWidthX     | 21     | number         | 0                                  |

## Local Subr INDEX

INDEX Data形式でCharStringsのサブルーチンが格納されている。

## CharStrings INDEX

INDEX Data形式でCharStringsのデータが格納されている。
CharStringsはグリフを描画するPostScriptバイナリである。
配列はGIDでアクセスし、GID 0(.notdef)から始まる。

* [https://adobe-type-tools.github.io/font-tech-notes/pdfs/5177.Type2.pdf](https://adobe-type-tools.github.io/font-tech-notes/pdfs/5177.Type2.pdf)
* [CFFラスタライズ](CFFラスタライズ)

## FDArray

Top DICTのFDArrayからオフセット指定され、DICT Data形式になっている。

## FDSelect

Top DICTのFDSelectからオフセット指定されている。
CIDフォントではGIDがFDSelect(FDArrayのインデックス)を持っている。
GIDとFDSelectを紐づけるのがFDSelectである。
オフセットから1バイト読み込みフォーマットを決定する。

Format0:

| タイプ | 名前         | 説明                                            |
|--------|--------------|-------------------------------------------------|
| byte   | format       | 0固定                                           |
| byte   | fds[nGlyphs] | FDSelectの配列、nGlyphsはGIDの数(.notdefを含む) |

Format3:

| タイプ | 名前            | 説明         |
|--------|-----------------|--------------|
| byte   | format          | 3固定        |
| ushort | nRanges         | Range3の個数 |
| struct | Range3[nRanges] | Range3の配列 |
| ushort | sentinel        | 最後のfirst  |

Range3:

| タイプ | 名前  | 説明       |
|--------|-------|------------|
| ushort | first | GID        |
| byte   | fd    | FDSelect値 |

Format3はFDSelect値が同じものが連続して格納されることを意味する。
連続するGIDの `Range3[n].first ～ (Range3[n + 1].first - 1)` までがRange3[n].fdのFDSelectを持つ。
最後のRange3は範囲外の参照となってしまうため、Format3に余分なsentinelが用意されているのである。
C言語のように配列の範囲外アクセスを行うことを前提としたフォーマットであるため、配列の範囲外アクセスをチェックするような言語では最後の要素を特殊扱いする必要がある。
FDSelectの読み込みは次のようになる。

```cs
var format = Read1Byte();
var fdselect = new byte[nGlyphs];

if (format == 0)
{
	for (var i = 0; i < nGlyphs; i++) fdselect[i] = Read1Byte();
}
else
{
	var count = 0;
	var nRanges = (ushort)Read2Byte();
	// 仕様ではRange3配列(firstとfd)→sentinelというレイアウトだが
	// first→fdとsentinelの配列と読み替えてアクセスする事で最後の要素判定を回避
	var first = (ushort)Read2Byte();
	for (var i = 0; i < nRanges; i++)
	{
		var fd = Read1Byte();
		var sentinel = (ushort)Read2Byte();
		for (var j = first; j < sentinel; j++) fdselect[count++] = fd;
		first = sentinel;
	}
}
```

