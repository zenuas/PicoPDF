# PicoPDF [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE)

PicoPDF is minimum report library  

## Description

PicoPDF named after the smallest SI prefix I know.  
can output a simple pdf.  
can't do difficult things.  
that's all it is.  

PicoPDFは知っている最小のSI接頭語にちなんで名付けた  
簡単なPDFを出力できる  
難しいことはできない  
ただそれだけ  

## Usage

```cs
var datas = new Data[] { ... };

var doc = new PicoPDF.Pdf.Document();
doc.FontRegister.RegistDirectory("your font directory");
var pagesection = PicoPDF.Binder.JsonLoader.Load("example.json");
var pages = PicoPDF.Binder.SectionBinder.Bind(pagesection, datas);
PicoPDF.Model.ModelMapping.Mapping(doc, pages);
doc.Save("example.pdf");
```

## Json definition file

PicoPDFはデータとjson定義ファイルにしたがってPDFを作成する。  
json定義ファイルには以下の4つのセクションとブレークキー(任意設定可)からなる。  

* DetailSection  
  明細行を表す、DetailSectionは必ず1つだけないといけない  
* HeaderSection  
  ページヘッダーや明細をグループ化した際のヘッダーとなる  
  ページ先頭やキーブレイク時に出力される  
* TotalSection  
  キーブレイク後に出力される  
  DetailSectionの直後に表示される  
* FooterSection  
  ページフッターやキーブレイク後に出力される  
  用紙の最下部に出力される  

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
  合計・カウント・平均を出力する。  
  マッパーが対応したプロパティアクセスである必要がある。  
  TotalSectionやFooterSectionに配置することを想定し、キーブレイクするとクリアされる。  
* LineElement  
  直線を出力する。  
* RectangleElement  
  長方形を出力する。中身は塗りつぶさない。  
* FillRectangleElement  
  長方形を出力し、中身を塗りつぶす。  
* ImageElement  
  画像を出力する。  
  jpegのみに対応。  

## Think about

キーブレイク時の改ページが全フッター出力後に起こってしまう。  
HeaderSectionにSummaryElementを配置すると必ずゼロになる。ヘッダーで合計表示ができない。  
テキスト出力はTrueTypeフォントしか対応していない。(ドキュメント直接操作すれば他フォントも利用可)  
サロゲートペアやRTLには対応していない。  
縦書きには対応していない。  
画像はjpegファイルにしか対応していない。(pngぐらい対応したい)  
任意のオブジェクトマッパーを利用できない。  
PDFへフォント埋め込やしおりやリンクには対応していない。  
PDFの暗号化は対応しない。  
