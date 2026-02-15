# PicoPDF [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)

PicoPDF is minimum report library  

## Description

PicoPDF named after the smallest SI prefix I know.  
can output a simple pdf.  
can't do difficult things.  
that's all it is.  

PicoPDFは知っている最小のSI接頭語にちなんで名付けた。  
簡単なPDFを出力できる。  
難しいことはできない。  
ただそれだけ。  

## Usage

```cs
var datas = new Data[] { ... };

PicoPDF.Pdf.PdfUtility.CreateDocument("example.json", datas).Save("example.pdf");
```

## Json definition file

PicoPDFはデータとjson定義ファイルにしたがってPDFを作成する。  
json定義ファイルには以下の4つのセクションとブレークキー(任意設定可)からなる。  

* DetailSection  
  明細行を表す、DetailSectionは必ず1つだけ必要。  
* HeaderSection  
  ページヘッダーや明細をグループ化した際のヘッダーとなる。  
  ページ先頭やキーブレイク時に出力される。  
* TotalSection  
  キーブレイク後に出力される。  
  DetailSectionの直後に表示される。  
* FooterSection  
  ページフッターやキーブレイク後に出力される。  
  用紙の最下部に出力される。  

表示タイミングは各セクションのプロパティで設定する。  

* Every  
  キーブレイクに限らず毎回表示される。  
* PageFirst  
  初回とページ後に表示される。  
  HeaderSection専用。  
* First  
  初回だけ表示される。  
  HeaderSection専用。  
* Last  
  最後だけ表示される。  
  TotalSectionとFooterSection専用。  

## Section elements

セクションには要素を配置することができる。  

* TextElement  
  固定のテキストを出力する。  
* BindElement  
  データを出力する。  
  マッパーが対応したプロパティアクセスである必要がある。  
* SummaryElement  
  合計・カウント・平均・最小・最大・ページ数を出力する。  
  マッパーが対応したプロパティアクセスである必要がある。  
* LineElement  
  直線を出力する。  
* RectangleElement  
  長方形を出力する。中身は塗りつぶさない。  
* FillRectangleElement  
  長方形を出力し、中身を塗りつぶす。  
* ImageElement  
  画像を出力する。  
  jpegとpngとbmpのみに対応。(一部フォーマットのみ対応)  

SummaryElementの集計方法はSummaryMethodプロパティで設定する。  

* Page  
  ページ内を集計対象とする。  
  セクションをまたがない。  
* CrossSectionPage  
  ページ内を集計対象とする。  
  セクションをまたいで集計する。  
* PageIncremental  
  ページ内の現在行までを集計対象とする。  
  セクションをまたがない。  
* CrossSectionPageIncremental  
  ページ内の現在行までを集計対象とする。  
* Group  
  同一セクション内を集計対象とする。  
  ページをまたいで集計する。  
* GroupIncremental  
  同一セクション内の現在行までを集計対象とする。  
  ページをまたいで集計する。  
* All  
  全ページの全セクションを集計対象とする。  
  セクションをまたいで集計する。  
* AllIncremental  
  全ページの全セクションの現在行までを集計対象とする。  

## Think about

キーブレイク時の改ページが全フッター出力後に起こってしまう。  
テキスト出力はOpenTypeフォント(TrueTypeとCFF)に対応している。  
RTLや縦書きには対応していない。  
しおりやリンクには対応していない。  
PDFの暗号化は対応しない。  
