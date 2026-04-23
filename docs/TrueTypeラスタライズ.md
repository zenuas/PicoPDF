# TrueTypeラスタライズ

PDFにフォントを埋め込むために必須ではないが、TrueTypeフォントをベクトルデータに変換しSVG形式に変換する方法を解説する。

* [mdn SVG](https://developer.mozilla.org/en-US/docs/Web/SVG){:target="_blank"}
* [Wikipedia SVG](https://en.wikipedia.org/wiki/SVG){:target="_blank"}

## glyfテーブルデータ

TrueTypeのglyfテーブルには[Simple glyph](https://learn.microsoft.com/ja-jp/typography/opentype/spec/glyf#simple-glyph-description){:target="_blank"}と[Composite glyph](https://learn.microsoft.com/ja-jp/typography/opentype/spec/glyf#composite-glyph-description){:target="_blank"}の2種類がある。
Simple glyphがグリフの座標データを持つもので、Composite glyphは複数のグリフを組み合わせて1つの文字を表現するものである。

Simple glyphであるかどうかはヘッダーの[numberOfContoursが-1](https://learn.microsoft.com/ja-jp/typography/opentype/spec/glyf#glyph-headers){:target="_blank"}であるかどうかで判断する。

## Simple glyph

Simple glyphはflags、xCoordinates、yCoordinatesの3組の配列で表現される。
numberOfContoursが組の数となる。

xCoordinates、yCoordinatesは直前の座標からの相対座標になる。
絶対座標に変換する場合は事前に変換しておくとよい。
座標変換は次のような畳み込みで解決できる。
```cs
int[] to_absolute(int[] xs) => xs[1..].Aggregate([xs[0]], (ys, a) => [.. ys, ys[^1] + a]);
```

endPtsOfContoursから組を取得して面を構成する。
面を構成するパスは閉じている必要があるため、最後の点から最初の点につなぐ必要がある。
SVGであれば[パスを閉じるコマンド](https://developer.mozilla.org/en-US/docs/Web/SVG/Reference/Attribute/d#closepath){:target="_blank"}がある。
```cs
var xs = to_absolute(xCoordinates);
var ys = to_absolute(yCoordinates);
var prev = 0;
for(var i = 0; i < numberOfContours; i++)
{
    var vs = xs[prev .. endPtsOfContours[i]]
        .Zip(ys[prev .. endPtsOfContours[i]])
        .Select(a => new Vector2(a.First, a.Second));
    if (vs.First() != vs.Last()) vs = [.. vs, vs.First()]; // 必要に応じて追加
    prev = endPtsOfContours[i] + 1;
}
```

グリフは直線と[2次ベジェ曲線](https://en.wikipedia.org/wiki/B%C3%A9zier_curve){:target="_blank"}で表現される。
直線か曲線かの判断はflagsのON_CURVE_POINT(線上の点)で判断する。
始点と終点が両方ON_CURVE_POINTであれば直線である。
終点がON_CURVE_POINTでない場合は2次ベジェ曲線の制御点となる。
制御点が2回連続して出現することがある。この場合は制御点の中間点を終点とした曲線にする必要がある。
下図は青点がON_CURVE_POINT、赤点が制御点、緑点が中間点である。

<svg width="219.88701" height="200" xmlns="http://www.w3.org/2000/svg">
    <!-- baseline -->
    <line x1="0" y1="187.9096" x2="219.88701" y2="187.9096" stroke="red" />
    <!-- あ -->
    <circle cx="116.83616" cy="200" r="4" fill="blue" />
    <circle cx="112.20339" cy="172.65536" r="4" fill="blue" />
    <circle cx="186.77966" cy="172.65536" r="4" fill="red" />
    <circle cx="186.77966" cy="130.1695" r="4" fill="blue" />
    <circle cx="186.77966" cy="107.570625" r="4" fill="red" />
    <circle cx="158.53107" cy="97.62712" r="4" fill="blue" />
    <circle cx="140.22598" cy="135.59323" r="4" fill="red" />
    <circle cx="108.757065" cy="163.38983" r="4" fill="green" />
    <circle cx="77.28814" cy="191.18645" r="4" fill="red" />
    <circle cx="47.23164" cy="191.18645" r="4" fill="blue" />
    <circle cx="29.604519" cy="191.18645" r="4" fill="red" />
    <circle cx="19.887005" cy="181.07346" r="4" fill="green" />
    <circle cx="10.169492" cy="170.96045" r="4" fill="red" />
    <circle cx="10.169492" cy="152.76837" r="4" fill="blue" />
    <circle cx="10.169492" cy="130.84746" r="4" fill="red" />
    <circle cx="25.706215" cy="111.63842" r="4" fill="green" />
    <circle cx="41.24294" cy="92.42938" r="4" fill="red" />
    <circle cx="66.77966" cy="80.90395" r="4" fill="blue" />
    <circle cx="66.44068" cy="71.18644" r="4" fill="red" />
    <circle cx="66.44068" cy="52.203384" r="4" fill="blue" />
    <circle cx="14.802259" cy="52.203384" r="4" fill="blue" />
    <circle cx="14.802259" cy="26.779663" r="4" fill="blue" />
    <circle cx="67.231636" cy="26.779663" r="4" fill="blue" />
    <circle cx="67.68362" cy="17.96611" r="4" fill="red" />
    <circle cx="68.24859" cy="0" r="4" fill="blue" />
    <circle cx="100.67796" cy="0" r="4" fill="blue" />
    <circle cx="100.45198" cy="11.299438" r="4" fill="red" />
    <circle cx="99.43503" cy="26.779663" r="4" fill="blue" />
    <circle cx="201.12994" cy="26.779663" r="4" fill="blue" />
    <circle cx="201.12994" cy="52.203384" r="4" fill="blue" />
    <circle cx="97.85311" cy="52.203384" r="4" fill="blue" />
    <circle cx="97.62712" cy="63.954803" r="4" fill="red" />
    <circle cx="97.62712" cy="71.18644" r="4" fill="blue" />
    <circle cx="111.52542" cy="68.47458" r="4" fill="red" />
    <circle cx="125.42373" cy="68.47458" r="4" fill="blue" />
    <circle cx="166.66667" cy="68.47458" r="4" fill="red" />
    <circle cx="193.27684" cy="84.971756" r="4" fill="green" />
    <circle cx="219.88701" cy="101.468925" r="4" fill="red" />
    <circle cx="219.88701" cy="131.75142" r="4" fill="blue" />
    <circle cx="219.88701" cy="164.85876" r="4" fill="red" />
    <circle cx="192.48587" cy="182.42938" r="4" fill="green" />
    <circle cx="165.08475" cy="200" r="4" fill="red" />
    <circle cx="119.77401" cy="200" r="4" fill="blue" />
    <circle cx="116.83616" cy="200" r="4" fill="blue" />
    <circle cx="128.70056" cy="92.42938" r="4" fill="blue" />
    <circle cx="127.45763" cy="92.20339" r="4" fill="red" />
    <circle cx="125.31074" cy="92.20339" r="4" fill="blue" />
    <circle cx="114.35028" cy="92.20339" r="4" fill="red" />
    <circle cx="97.62712" cy="96.27119" r="4" fill="blue" />
    <circle cx="98.757065" cy="115.819214" r="4" fill="red" />
    <circle cx="101.920906" cy="132.54237" r="4" fill="blue" />
    <circle cx="117.85311" cy="115.706215" r="4" fill="red" />
    <circle cx="128.70056" cy="92.42938" r="4" fill="blue" />
    <circle cx="68.81356" cy="109.49153" r="4" fill="blue" />
    <circle cx="40" cy="128.81357" r="4" fill="red" />
    <circle cx="40" cy="150.1695" r="4" fill="blue" />
    <circle cx="40" cy="164.51978" r="4" fill="red" />
    <circle cx="50.16949" cy="164.51978" r="4" fill="blue" />
    <circle cx="61.355934" cy="164.51978" r="4" fill="red" />
    <circle cx="76.72317" cy="153.89832" r="4" fill="blue" />
    <circle cx="71.41243" cy="134.46329" r="4" fill="red" />
    <circle cx="68.81356" cy="109.49153" r="4" fill="blue" />
    <path stroke="#FFFFFF" fill="transparent" fill-rule="evenodd" 
       d="
          M 116.83616 200
          L 112.20339 172.65536
          Q 186.77966 172.65536, 186.77966 130.1695
          Q 186.77966 107.570625, 158.53107 97.62712
          Q 140.22598 135.59323, 108.757065 163.38983
          Q 77.28814 191.18645, 47.23164 191.18645
          Q 29.604519 191.18645, 19.887005 181.07346
          Q 10.169492 170.96045, 10.169492 152.76837
          Q 10.169492 130.84746, 25.706215 111.63842
          Q 41.24294 92.42938, 66.77966 80.90395
          Q 66.44068 71.18644, 66.44068 52.203384
          L 14.802259 52.203384
          L 14.802259 26.779663
          L 67.231636 26.779663
          Q 67.68362 17.96611, 68.24859 0
          L 100.67796 0
          Q 100.45198 11.299438, 99.43503 26.779663
          L 201.12994 26.779663
          L 201.12994 52.203384
          L 97.85311 52.203384
          Q 97.62712 63.954803, 97.62712 71.18644
          Q 111.52542 68.47458, 125.42373 68.47458
          Q 166.66667 68.47458, 193.27684 84.971756
          Q 219.88701 101.468925, 219.88701 131.75142
          Q 219.88701 164.85876, 192.48587 182.42938
          Q 165.08475 200, 119.77401 200
          L 116.83616 200
          Z
          M 128.70056 92.42938
          Q 127.45763 92.20339, 125.31074 92.20339
          Q 114.35028 92.20339, 97.62712 96.27119
          Q 98.757065 115.819214, 101.920906 132.54237
          Q 117.85311 115.706215, 128.70056 92.42938
          Z
          M 68.81356 109.49153
          Q 40 128.81357, 40 150.1695
          Q 40 164.51978, 50.16949 164.51978
          Q 61.355934 164.51978, 76.72317 153.89832
          Q 71.41243 134.46329, 68.81356 109.49153
          Z" />
</svg>

文字の下側に引かれた赤線はベースラインである。
yCoordinatesの座標はベースラインより上がプラス、下がマイナスとなる。(数学の座標と同じ、x軸がベースライン)
日本語グリフではベースラインより下側を使用することは少ないが、アルファベットのjなどはベースラインより下側に描画する。
xCoordinatesは通常はプラスであることが多いが、マイナスにもなりうる。

<svg width="69.13087" height="200" xmlns="http://www.w3.org/2000/svg">
    <!-- baseline -->
    <line x1="0" y1="157.24275" x2="69.13087" y2="157.24275" stroke="red" />
    <!-- j -->
    <circle cx="69.13087" cy="27.072922" r="4" fill="blue" />
    <circle cx="36.263733" cy="27.072922" r="4" fill="blue" />
    <circle cx="36.263733" cy="0" r="4" fill="blue" />
    <circle cx="69.13087" cy="0" r="4" fill="blue" />
    <circle cx="69.13087" cy="27.072922" r="4" fill="blue" />
    <circle cx="68.73126" cy="158.24176" r="4" fill="blue" />
    <circle cx="68.73126" cy="177.62238" r="4" fill="red" />
    <circle cx="57.542454" cy="188.81119" r="4" fill="green" />
    <circle cx="46.353645" cy="200" r="4" fill="red" />
    <circle cx="27.272726" cy="200" r="4" fill="blue" />
    <circle cx="10.48951" cy="200" r="4" fill="red" />
    <circle cx="0" cy="197.30269" r="4" fill="blue" />
    <circle cx="0" cy="174.52547" r="4" fill="blue" />
    <circle cx="1.4985013" cy="174.52547" r="4" fill="blue" />
    <circle cx="10.889111" cy="177.72227" r="4" fill="red" />
    <circle cx="18.081917" cy="177.72227" r="4" fill="blue" />
    <circle cx="28.771227" cy="177.72227" r="4" fill="red" />
    <circle cx="33.41658" cy="171.97801" r="4" fill="green" />
    <circle cx="38.061935" cy="166.23376" r="4" fill="red" />
    <circle cx="38.061935" cy="148.65134" r="4" fill="blue" />
    <circle cx="38.061935" cy="61.838158" r="4" fill="blue" />
    <circle cx="15.084914" cy="61.838158" r="4" fill="blue" />
    <circle cx="15.084914" cy="41.55844" r="4" fill="blue" />
    <circle cx="68.73126" cy="41.55844" r="4" fill="blue" />
    <circle cx="68.73126" cy="158.24176" r="4" fill="blue" />
    <path stroke="#FFFFFF" fill="transparent" fill-rule="evenodd" 
       d="
          M 69.13087 27.072922
          L 36.263733 27.072922
          L 36.263733 0
          L 69.13087 0
          L 69.13087 27.072922
          Z
          M 68.73126 158.24176
          Q 68.73126 177.62238, 57.542454 188.81119
          Q 46.353645 200, 27.272726 200
          Q 10.48951 200, 0 197.30269
          L 0 174.52547
          L 1.4985013 174.52547
          Q 10.889111 177.72227, 18.081917 177.72227
          Q 28.771227 177.72227, 33.41658 171.97801
          Q 38.061935 166.23376, 38.061935 148.65134
          L 38.061935 61.838158
          L 15.084914 61.838158
          L 15.084914 41.55844
          L 68.73126 41.55844
          L 68.73126 158.24176
          Z" />
</svg>

## Composite glyph

Composite glyphは複数のグリフのGIDを指定する。
指定したGIDもComposite glyphである可能性がある。循環してはいけない。
各グリフを移動、回転、拡大縮小して組み合わせる。

argument1、argument2で移動を表す。
フラグのうちWE_HAVE_A_SCALE、WE_HAVE_AN_X_AND_Y_SCALE、WE_HAVE_A_TWO_BY_TWOは排他フラグとなる。
アフィン変換行列を作って座標移動するとよい。

```cs
var m = Matrix3x2.Identity;
m.Translation = new Vector2(aArgument1, argument2);

if (flags.HasFlag(WE_HAVE_A_SCALE))
{
    m.M11 = m.M22 = scale;
}
else if (flags.HasFlag(WE_HAVE_AN_X_AND_Y_SCALE))
{
    m.M11 = xscale;
    m.M22 = yscale;
}
else if (flags.HasFlag(WE_HAVE_A_TWO_BY_TWO))
{
    m.M11 = xscale;
    m.M12 = scale01;
    m.M21 = scale10;
    m.M22 = yscale;
}

var vec = Vector2.Transform(v, m); // 各種座標vをアフィン変換
```

scale、xscale、yscale、scale01、scale10は[F2DOT14](https://learn.microsoft.com/ja-jp/typography/opentype/spec/otff#data-types){:target="_blank"}である。
F2DOT14は上位2bitが整数部、下位14bitが小数部の固定小数点形式であるため、floatへ変換する場合は2 ^ 14(=16384)で割る必要がある。
