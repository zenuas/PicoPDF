# PDFのフォント

PDFで使用できるフォントは複数種類ある。
OpenType(TrueType、CFF)を使用する場合はType 0フォントを使用する。

| 名前             | 説明                                                                                                                                                                                     |
|------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Type 0フォント   | 複合フォント、CIDフォントなどを持つ                                                                                                                                                      |
| Type 1フォント   | タイプ1フォント、使用できるのは1バイト範囲のみ、[Adobe製品ではサポートが終了している](https://helpx.adobe.com/jp/fonts/kb/postscript-type-1-fonts-end-of-support.html){:target="_blank"} |
| MMType 1フォント | マルチプルマスタータイプ1フォント、[Adobe製品ではサポートが終了している](https://helpx.adobe.com/jp/fonts/kb/postscript-type-1-fonts-end-of-support.html){:target="_blank"}              |
| Type 3フォント   | ビットマップ画像やPostScript形式                                                                                                                                                         |
| TrueType         | TrueType、使用できるのは1バイト範囲のみ、2バイト以上の文字を扱う場合はType 0フォントを利用する                                                                                           |
| CIDFontType0     | OpenType(CFF)形式のCIDフォント                                                                                                                                                           |
| CIDFontType2     | OpenType(TrueType)形式のCIDフォント                                                                                                                                                      |

## Type 0フォント

Type 0フォントは複数のフォントを持つことができるフォントである。
ただし、PDFにおいては1つのCIDFontフォントのみとなっている。
OpenTypeフォントを扱うためのフォントと考えて差し支えない。

| キー            | 型             | 説明                                                                                                                                                                                                            |
|-----------------|----------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Type            | name           | 必須、/Font固定                                                                                                                                                                                                 |
| Subtype         | name           | 必須、/Type0固定                                                                                                                                                                                                |
| BaseFont        | name           | 必須、フォントのPostScript名<br>CIDFontType0のCIDフォントを持つ場合は「CIDフォントのBaseFont名-Encodingエントリ名かCMapName名」を指定<br>CIDFontType2のCIDフォントを持つ場合は「CIDフォントのBaseFont名」を指定 |
| Encoding        | name or stream | 必須、定義済みのCMap名                                                                                                                                                                                          |
| DescendantFonts | array          | 必須、CIDFont辞書、必ず要素が1つの配列にする                                                                                                                                                                    |
| ToUnicode       | stream         | CIDをUnicodeコード値にマッピングするCMapストリーム                                                                                                                                                              |

### DescendantFonts

CIDFont辞書情報になる。
OpenTypeのTrueType形式、CFF形式で設定する内容が異なる。

| キー           | 型                                   | 説明                                                                                  |
|----------------|--------------------------------------|---------------------------------------------------------------------------------------|
| Type           | name                                 | 必須、/Font固定                                                                       |
| Subtype        | name                                 | 必須、OpenType(CFF)形式の場合/CIDFontType0、OpenType(TrueType)形式の場合/CIDFontType2 |
| BaseFont       | name                                 | 必須、PostScript名                                                                    |
| CIDSystemInfo  | dictionary                           | 必須、CIDFontの文字コレクションを定義するエントリを含む辞書                           |
| FontDescriptor | dictionary(間接参照でないといけない) | 必須、CIDFontのグリフ幅以外のデフォルトメトリクスを記述するフォント記述子             |
| DW             | integer                              | デフォルトの幅、初期値は1000                                                          |
| W              | array                                | CIDFont内のグリフの幅                                                                 |

OpenTypeにも[文字幅の情報を持っている](OpenTypeフォント抽出#TTF、CFFの共通部分抽出)が、別途Wで指定する必要がある。

### CIDSystemInfo

PicoPDFではOpenTypeの文字は文字コードで指定せず、[GID](OpenTypeフォント抽出#フォントの構造)で指定するためAdobe-Identity-0のみを使用する。

| キー       | 型           | 説明                                     |
|------------|--------------|------------------------------------------|
| Registry   | ASCII string | 必須、文字コレクションの発行元、例:Adobe |
| Ordering   | ASCII string | 必須、文字コレクションの名前、例:Japan1  |
| Supplement | integer      | 必須、文字コレクションの補足番号         |

### FontDescriptor

Type 0フォントで必須のもののみ説明する。

| キー        | 型        | 説明                                                                 |
|-------------|-----------|----------------------------------------------------------------------|
| Type        | name      | 必須、/FontDescriptor固定                                            |
| FontName    | name      | 必須、PostScript名                                                   |
| FontBBox    | rectangle | 必須、全てのグリフがおさまる最小矩形                                 |
| ItalicAngle | number    | 必須、斜体の角度、反時計回りの角度で指定                             |
| Ascent      | number    | 必須、ベースラインからの最大高さ                                     |
| Descent     | number    | 必須、ベースラインからの最大深度、負の数となる                       |
| CapHeight   | number    | ラテン文字を含むフォントで必須、水平な大文字のベースラインからの高さ |
| StemV       | number    | 必須、垂直ステムの横幅                                               |
| FontFile2   | stream    | TrueType形式フォントを埋め込んだ場合のストリーム                     |
| FontFile3   | stream    | CFF形式フォントを埋め込んだ場合のストリーム                          |

# サブセットフォント

OpenType形式のフォントをPDFに埋め込むことができる。
ただし、日本語のフォントファイルであれば数万種が格納されているため、PDFファイルサイズが肥大化する。
そのため、[サブセットフォント](OpenTypeフォント抽出)化して埋め込みたい。

下記はサブセットフォント化したグリフのみを含んだサンプルである。

* [TTF形式フォント埋め込み、Noto Sans JP](sample/subset-ttf.pdf){:target="_blank"}
* [CFF形式フォント埋め込み、Noto Sans CJK JP](sample/subset-cff.pdf){:target="_blank"}
