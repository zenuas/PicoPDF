# OpenTypeフォント抽出

PDFにフォントを埋め込むためにOpenTypeから使用するグリフのみを抽出してPDFに埋め込む。  
フォーマットの仕様はMicrosoft、Appleの仕様を参照すること。  
ここではPDFにフォントを埋め込むための抽出処理について解説する。  

* [https://learn.microsoft.com/ja-jp/typography/opentype/spec/](https://learn.microsoft.com/ja-jp/typography/opentype/spec/){:target="_blank"}  
* [https://developer.apple.com/fonts/TrueType-Reference-Manual/](https://developer.apple.com/fonts/TrueType-Reference-Manual/){:target="_blank"}  

## OpenType

OpenTypeフォントはTrueType(TTF)形式とCompact Font Format(CFF)形式のデータを格納できるようにしたものである。  
CFF形式のデータは[sfntフォーマット](https://en.wikipedia.org/wiki/SFNT){:target="_blank"}のCFFテーブルとして格納される。  

TTF形式とCFF形式では次の違いがある。  

| 相違点             | TrueType(TTF)形式                              | Compact Font Format(CFF)形式             |
|--------------------|------------------------------------------------|------------------------------------------|
| SFNTバージョン     | 0x00010000 または 0x74727565(true)             | 0x4F54544F(OTTO)                         |
| 共通テーブル       | cmap、head、hhea、hmtx、maxp、name、OS/2、post | cmap、head、hhea、hmtx、maxp、name、post |
| グリフ格納テーブル | glyf、loca                                     | CFF または CFF2                          |
| 拡張子             | .TTF、.OTF                                     | .OTF                                     |
| アウトライン       | 2次ベジェ曲線                                  | PostScriptベース、3次ベジェ曲線          |

[Microsoftの仕様ではTTFのSFNTバージョンは0x00010000](https://learn.microsoft.com/ja-jp/typography/opentype/spec/otff#font-tables){:target="_blank"}のみだが、[Appleの仕様では0x74727565](https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6.html){:target="_blank"}がある。  
[Microsoftの仕様ではOS/2が必須テーブル](https://learn.microsoft.com/ja-jp/typography/opentype/spec/otff#font-tables){:target="_blank"}とされているが、[Appleの仕様ではオプション](https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6.html){:target="_blank"}とされている。  
Macにインストールされているフォントでは0x74727565やOS/2テーブル無しのTTFフォントがある。  

OpenTypeとTrueTypeの比較がされることがあるが、一般にOpenTypeとされているのはCFFフォントであることが多い。  

1つのフォントファイルに複数のフォントファイルを格納できるフォントコレクション(ヘッダ ttcf、拡張子 .TTC)がある。  

### CFFテーブル

CFFフォントではCFFテーブルにCompact Font Format形式のデータが格納されている。  

* [http://partners.adobe.com/public/developer/en/font/5176.CFF.pdf](http://partners.adobe.com/public/developer/en/font/5176.CFF.pdf){:target="_blank"}  
* [http://partners.adobe.com/public/developer/en/font/5177.Type2.pdf](http://partners.adobe.com/public/developer/en/font/5177.Type2.pdf){:target="_blank"}  

## フォントの構造

コンピュータでは文字はUnicodeなどのコードポイント(文字コード)で現わされる。  
フォントファイル内では文字コードではなくGlyph ID(GID)で格納される。  
GIDは0から連番で始まり、GIDの0は.notdefというフォントファイル内に文字が無いことを現わす。  
これは同じ文字コードでも異なるグリフが割り当たることがあるためである。  
文字コードとGIDを変換するのがcmapである。  

```mermaid
---
title: 文字とGIDの関係
---
graph LR;
    subgraph char ["文字"]
      A;
      B;
      C;
      あ;
    end
    
    subgraph code ["文字コード"]
      U+0041;
      U+0042;
      U+0043;
      U+3042;
    end
    
    subgraph gid ["GID"]
      0["0 (.notdef)"];
      1;
      2;
      3;
      4;
    end
    
    A  --encoding--> U+0041;
    B  --encoding--> U+0042;
    C  --encoding--> U+0043;
    あ --encoding--> U+3042;
    
    U+0041 --cmap--> 1;
    U+0042 --cmap--> 2;
    U+0043 --cmap--> 3;
    U+3042 --cmap--> 4;
```

## フォント抽出

フォントファイルには多数のグリフデータが格納されている。  
日本語のフォントファイルであれば数万種が格納されている。  
PDFにフォントファイルを埋め込む際に使用しない文字のグリフデータまで含めるとファイルサイズが肥大化する。  
そのためPDFファイル内で使用しているグリフデータのみを抽出したフォントファイルを作成して埋め込みたい。  


```mermaid
---
title: 「a」と「あ」のグリフデータ抽出
---
graph LR;
    subgraph old_gid ["元のGID"]
      old0["0 (.notdef)"];
      old1["1 (A)"];
      old2["2 (B)"];
      old3["3 (C)"];
      old4["4 (あ)"];
    end
    
    subgraph new_gid ["新しいGID"]
      new0["0 (.notdef)"];
      new1["1 (A)"];
      new2["2 (あ)"];
    end
    
    old0 --抽出--> new0;
    old1 --抽出--> new1;
    old4 --抽出--> new2;
```

## TTF、CFFの共通部分抽出

グリフデータを抽出にあたり、必須テーブルのグリフ数、グリフデータに関するパラメータを変更する必要がある。  

| テーブル                                                                                  | 抽出方法                                  |
|-------------------------------------------------------------------------------------------|-------------------------------------------|
| [cmap](#cmapの抽出)                                                                       | 抽出したGIDに合わせた変換が必要           |
| [head](https://learn.microsoft.com/ja-jp/typography/opentype/spec/head){:target="_blank"} | 変換不要                                  |
| [hhea](https://learn.microsoft.com/ja-jp/typography/opentype/spec/hhea){:target="_blank"} | numberOfHMetricsを抽出したグリフ数に設定  |
| [hmtx](https://learn.microsoft.com/ja-jp/typography/opentype/spec/hmtx){:target="_blank"} | hMetricsを抽出したGIDに合わせた変換が必要 |
| [maxp](https://learn.microsoft.com/ja-jp/typography/opentype/spec/maxp){:target="_blank"} | numGlyphsを抽出したグリフ数に設定         |
| [name](https://learn.microsoft.com/ja-jp/typography/opentype/spec/name){:target="_blank"} | 変換不要                                  |
| [OS/2](https://learn.microsoft.com/ja-jp/typography/opentype/spec/os2){:target="_blank"}  | 変換不要                                  |
| [post](https://learn.microsoft.com/ja-jp/typography/opentype/spec/post){:target="_blank"} | 変換不要                                  |

### cmapの抽出

PDFでOpenTypeフォントを使用する場合はGIDを指定することになる。  
OpenType内のcmapテーブルがあるが、PDFではTo Unicode CMapを利用する。  
そのため文字コードからGIDへの変換を行うcmapは重要ではなく、UVSを実装する意味は薄い。  
最低限のcmapのみを抽出すればよい。  

* [https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap){:target="_blank"}  
* [https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6cmap.html](https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6cmap.html){:target="_blank"}  

#### プラットフォーム

フォントデータ内には様々なプラットフォームやエンコード向けに複数のcmapが含まれている。  
プラットフォームは次の種類があるがPDFに埋め込む場合はUnicodeにのみ対応すれば十分である。  

| プラットフォーム | 名前         |
|------------------|--------------|
| 0                | Unicode      |
| 1                | Macintosh    |
| 2                | ISO (非推奨) |
| 3                | Windows      |
| 4                | Custom       |

#### プラットフォームUnicodeにおけるエンコーディング

プラットフォームにUnicodeを選択した場合は次のエンコーディングを指定する必要がある。  
PDF埋め込みにおいて、採用すべきは基本多言語面のみであれば3、サロゲートペアを扱う場合は4までである。  

| エンコーディング | 名前                         | 補足             |
|------------------|------------------------------|------------------|
| 0                | Unicode 1.0 (非推奨)         |                  |
| 1                | Unicode 1.1 (非推奨)         |                  |
| 2                | ISO/IEC 10646 (非推奨)       |                  |
| 3                | Unicode 2.0 基本多言語面のみ | U+0000～U+FFFF   |
| 4                | Unicode 2.0                  | U+0000～U+10FFFF |
| 5                | Unicode variation sequences  | Format 14専用    |
| 6                | Unicode full repertoire      | Format 13専用    |

#### フォーマット

日本語のフォントファイルであればFormat 12のみ実装すればよい。  

| サブテーブル                                                                                                                            | 文字コード範囲                 | 格納可能グリフ数 |
|-----------------------------------------------------------------------------------------------------------------------------------------|--------------------------------|-----------------:|
| [Format 0](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-0-byte-encoding-table){:target="_blank"}              | U+0000～U+00FF                 |            256個 |
| [Format 2](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-2-high-byte-mapping-through-table){:target="_blank"}  | U+0000～U+FFFF                 |          65536個 |
| [Format 4](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-4-segment-mapping-to-delta-values){:target="_blank"}  | U+0000～U+FFFF                 |          65536個 |
| [Format 6](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-6-trimmed-table-mapping){:target="_blank"}            | U+0000～U+FFFF(連続したコード) |          65536個 |
| [Format 8](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-8-mixed-16-bit-and-32-bit-coverage){:target="_blank"} | U+0000～U+10FFFF               |          65536個 |
| [Format 10](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-10-trimmed-array){:target="_blank"}                  | 32bit(使用されない)            |          65536個 |
| [Format 12](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-12-segmented-coverage){:target="_blank"}             | U+0000～U+10FFFF               |          65536個 |
| [Format 13](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-13-many-to-one-range-mappings){:target="_blank"}     | 32bit(多対一)                  |          65536個 |
| [Format 14](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#format-14-unicode-variation-sequences){:target="_blank"}    | UVS(SVS+IVS、異字体)           |          65536個 |

1つのフォントファイルに格納できるグリフ数は65536個が上限である。  
GID 0は.notdefになるため、実際に使用できるグリフ数は上記の格納グリフ数より1個少なくなる。  

#### 補足:Windows環境での単体フォントファイル

PDF埋め込みではなくWindows環境で単体のフォントファイルとして扱うのであればプラットフォーム 3:Windowsを含む必要がある。  
最低限エンコーディング 1、Format 4を含まないと有効なフォントファイルとみなされない。  
それ以外を実装する意味はないであろう。  

| エンコーディング | 名前                    | 補足      |
|------------------|-------------------------|-----------|
| 0                | Symbol                  |           |
| 1                | Unicode 基本多言語面    | Format 4  |
| 2                | ShiftJIS                |           |
| 3                | PRC                     |           |
| 4                | Big5                    |           |
| 5                | Wansung                 |           |
| 6                | Johab                   |           |
| 7                | 予約済み                |           |
| 8                | 予約済み                |           |
| 9                | 予約済み                |           |
| 10               | Unicode full repertoire | Format 12 |

* [https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#windows-platform-platform-id--3](https://learn.microsoft.com/ja-jp/typography/opentype/spec/cmap#windows-platform-platform-id--3){:target="_blank"}  

## TTFのフォント抽出

TTF形式はグリフ数分のglyf、locaテーブルを抽出する必要がある。  
locaの配列の差分がglyfの長さになるため、glyfをGIDの昇順に出力する。  
関連するテーブルをまとめると以下のようになる。  

```mermaid
graph LR;
    subgraph head
        indexToLocFormat;
    end
    
    subgraph maxp
        numGlyphs;
    end
    
    subgraph hhea
        numberOfHMetrics;
    end
    
    subgraph hmtx
        hMetrics["hMetrics[numGlyphs]"];
        leftSideBearings["leftSideBearings[numGlyphs - numberOfHMetrics]"];
    end
    
    subgraph loca
        direction LR;
        offsets0["offsets[0]"];
        offsets1["offsets[1]"];
        offsets2["offsets[2]"];
        offsets3["offsets[3]"];
        offsetsx["offsets[...]"];
        offsetslast["offsets[numGlyphs + 1]"];
    end
    
    subgraph glyf
        direction LR;
        glyf0["GID 0 (.notdef)"];
        glyf1["GID 1"];
        glyf2["GID 2"];
        glyflast["GID numGlyphs"];
    end
    
    numGlyphs --> hMetrics;
    numGlyphs --> leftSideBearings;
    numGlyphs --> loca;
    numberOfHMetrics --> leftSideBearings;
    indexToLocFormat --> loca;
    offsets1    --"offsets[0]～offsets[1]の差分"--> glyf0;
    offsets2    --"offsets[1]～offsets[2]の差分"--> glyf1;
    offsets3    --"offsets[2]～offsets[3]の差分"--> glyf2;
    offsetslast --"offsets[numGlyphs]～offsets[numGlyphs + 1]の差分"--> glyflast;
```

### glyfの抽出

グリフデータにはSimple glyphとComposite glyphがある。  
どちらも座標データのみであるため、抽出を行うだけであれば中身を解析する必要はない。  

headのindexToLocFormatが0の場合、locaは実際のオフセットを2で割った値が格納されているため、2バイトアライメントにする必要がある。  

## CFFのフォント抽出

CFF形式はCFFテーブルを抽出する必要がある。  
CFFテーブルにはCompact Font Formatのファイルが入っている。  
CFF形式については[CFFデータ読み込み](CFFデータ読み込み)にて解説する。  

## カラーグリフについて

## カラーグリフのフォント抽出
