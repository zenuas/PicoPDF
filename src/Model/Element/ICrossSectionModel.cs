namespace PicoPDF.Model.Element;

public interface ICrossSectionModel
{
    public SectionModel? TargetModel { get; set; }

    public void UpdatePosition(SectionModel current);
}
