# TrueTypeデータ読み込み

TTF形式はグリフ数分のglyf、locaテーブルを抽出する必要がある。
locaテーブルにはglyfテーブルの長さの情報が入っている。
glyfテーブルにはグリフのアウトライン情報が格納されている。
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

## locaテーブル

locaの配列の差分がglyfの長さになる。
アウトライン情報がない場合は差分がゼロ(offsets[GID] == offsets[GID + 1])になる。
headのindexToLocFormatが0の場合、locaは実際のオフセットを2で割った値が格納されている。

## glyfテーブル

グリフデータにはSimple glyphとComposite glyphがある。
どちらも座標データのみであるため、抽出を行うだけであれば中身を解析する必要はない。
headのindexToLocFormatが0の場合、2バイトアライメントにする必要がある。

* [TrueTypeラスタライズ](TrueTypeラスタライズ)
