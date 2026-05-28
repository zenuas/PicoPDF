# PDF作成

PDFの仕様はAdobeが開発し、現在はISOで標準化されている。
フォーマットの仕様はAdobeの仕様を参照すること。
ここでは最低限のPDF作成方法について解説する。

[https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/pdfreference1.7old.pdf](https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/pdfreference1.7old.pdf){:target="_blank"}
[https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/PDF32000_2008.pdf](https://opensource.adobe.com/dc-acrobat-sdk-docs/pdfstandards/PDF32000_2008.pdf){:target="_blank"}

[https://pdfa.org/resource/iso-32000-1/](https://pdfa.org/resource/iso-32000-1/){:target="_blank"}
[https://pdfa.org/resource/iso-32000-2/](https://pdfa.org/resource/iso-32000-2/){:target="_blank"}

## PicoPDFのバージョン

PicoPDFではVersion 1.7を想定する。

## PicoPDFの出力

PicoPDFではPDF出力時にオプションを設定することができる。
PdfExportOptionを設定することでテキストエディタでも構造が確認できるようになる。

| オプション                | 通常利用時の推奨値 | デバッグ時の推奨値 | 説明                                                                               |
|---------------------------|--------------------|--------------------|------------------------------------------------------------------------------------|
| Debug                     | false              | true               | コメントでデバッグ情報を出力                                                       |
| AppendCIDToUnicode        | true               | true               | テキストをコピー時したときに正しくコピーする                                       |
| ContentsStreamDeflate     | true               | false              | ストリームの圧縮、圧縮しない方がエディタで確認しやすい                             |
| JpegStreamDeflate         | true               | true               | JPEG画像の圧縮、圧縮しなくてもエディタで読み込みは不可                             |
| ImageStreamDeflate        | true               | true               | JPEG以外の画像の圧縮、圧縮しなくてもエディタで読み込みは不可                       |
| CMapStreamDeflate         | true               | false              | テキストをコピー時したときに正しくコピーする、圧縮しない方がエディタで確認しやすい |
| OutputCrossReferenceTable | true               | false              | クロスリファレンステーブル出力、エディタで編集するならない方がよい                 |
| PointFormat               | F%                 | F7                 | 小数点以下桁数、固定にしておいた方がエディタで確認しやすい                         |
