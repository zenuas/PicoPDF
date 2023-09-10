namespace PicoPDF.Mapper;

public class ObjectMapper
{
    /*
     * ISection.Elements からBindElementを取得する
     * BindElement.Name でTからFetchできるものを集めたFunc<T,R>の配列を得る
     * 1セクションごとに (string BindName, string Format, Func<T, object> Fetch)[] にまとめる
     * 
     * BindElementを取得する所はSection名前空間で解決すべき問題だし
     * Fetchを集める所だけをObjectMapperで切り出すべきか？
     * 不要ではないか？
     */
}
