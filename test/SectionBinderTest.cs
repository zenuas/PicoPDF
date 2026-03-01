using Binder;
using Binder.Data;
using Mina.Extension;
using PicoPDF.Loader.Sections;
using System;
using Xunit;

namespace PicoPDF.Test;

[Collection("SectionBinder")]
public class SectionBinderTest
{
    [Fact]
    public void GetSectionInfo_DetailOnly()
    {
        var subsection = new DetailSection() { Name = "detail", Height = 100 };
        var sections = SectionBinder.GetSectionInfo(subsection, null);

        Assert.Equal(sections.Headers.Length, 0);
        Assert.Equal(sections.Footers.Length, 0);
        Assert.Equal(sections.Detail, subsection);
        Assert.Equal(sections.BreakKeys.Length, 0);
    }

    [Fact]
    public void GetSectionInfo_Single()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var subsection = new DetailSection() { Name = "detail", Height = 100 };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 1);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Footers.Length, 0);
        Assert.Equal(sections.Detail, subsection);
        Assert.Equal(sections.BreakKeys.Length, 0);
    }

    [Fact]
    public void GetSectionInfo_Depth1_DetailOnly()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            SubSection = detail,
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 1);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Footers.Length, 0);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys.Length, 0);
    }

    [Fact]
    public void GetSectionInfo_Depth1_BreakKyeOnly()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            SubSection = detail,
            BreakKey = "break",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 1);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Footers.Length, 0);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break" });
    }

    [Fact]
    public void GetSectionInfo_Depth1()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = detail,
            BreakKey = "break",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 2);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Headers[1].Section, subsection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[1].BreakKey, "break");
        Assert.Equal(sections.Headers[1].BreakCount, 1);
        Assert.Equal(sections.Footers.Length, 1);
        Assert.Equal(sections.Footers[0].Section, subsection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[0].Depth, 1);
        Assert.Equal(sections.Footers[0].BreakKey, "break");
        Assert.Equal(sections.Footers[0].BreakCount, 1);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break" });
    }

    [Fact]
    public void GetSectionInfo_Depth2()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = new HeaderSection() { Name = "header2", Height = 100 },
                Footer = new FooterSection() { Name = "footer2", Height = 100, PageBreak = false },
                SubSection = detail,
                BreakKey = "break2",
            },
            BreakKey = "break1",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 3);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Headers[1].Section, subsection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[1].BreakKey, "break1");
        Assert.Equal(sections.Headers[1].BreakCount, 1);
        Assert.Equal(sections.Headers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[2].BreakKey, "break2");
        Assert.Equal(sections.Headers[2].BreakCount, 2);
        Assert.Equal(sections.Footers.Length, 2);
        Assert.Equal(sections.Footers[0].Section, subsection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[0].Depth, 1);
        Assert.Equal(sections.Footers[0].BreakKey, "break1");
        Assert.Equal(sections.Footers[0].BreakCount, 1);
        Assert.Equal(sections.Footers[1].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[1].Depth, 2);
        Assert.Equal(sections.Footers[1].BreakKey, "break2");
        Assert.Equal(sections.Footers[1].BreakCount, 2);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2" });
    }

    [Fact]
    public void GetSectionInfo_Depth3()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = new HeaderSection() { Name = "header2", Height = 100 },
                Footer = new FooterSection() { Name = "footer2", Height = 100, PageBreak = false },
                SubSection = new Section()
                {
                    Header = new HeaderSection() { Name = "header3", Height = 100 },
                    Footer = new FooterSection() { Name = "footer3", Height = 100, PageBreak = false },
                    SubSection = detail,
                    BreakKey = "break3",
                },
                BreakKey = "break2",
            },
            BreakKey = "break1",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 4);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Headers[1].Section, subsection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[1].BreakKey, "break1");
        Assert.Equal(sections.Headers[1].BreakCount, 1);
        Assert.Equal(sections.Headers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[2].BreakKey, "break2");
        Assert.Equal(sections.Headers[2].BreakCount, 2);
        Assert.Equal(sections.Headers[3].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[3].Depth, 3);
        Assert.Equal(sections.Headers[3].BreakKey, "break3");
        Assert.Equal(sections.Headers[3].BreakCount, 3);
        Assert.Equal(sections.Footers.Length, 3);
        Assert.Equal(sections.Footers[0].Section, subsection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[0].Depth, 1);
        Assert.Equal(sections.Footers[0].BreakKey, "break1");
        Assert.Equal(sections.Footers[0].BreakCount, 1);
        Assert.Equal(sections.Footers[1].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[1].Depth, 2);
        Assert.Equal(sections.Footers[1].BreakKey, "break2");
        Assert.Equal(sections.Footers[1].BreakCount, 2);
        Assert.Equal(sections.Footers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[2].Depth, 3);
        Assert.Equal(sections.Footers[2].BreakKey, "break3");
        Assert.Equal(sections.Footers[2].BreakCount, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2", "break3" });
    }

    [Fact]
    public void GetSectionInfo_Depth3_Sub2NoBreak()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = new HeaderSection() { Name = "header2", Height = 100 },
                Footer = new FooterSection() { Name = "footer2", Height = 100, PageBreak = false },
                SubSection = new Section()
                {
                    Header = new HeaderSection() { Name = "header3", Height = 100 },
                    Footer = new FooterSection() { Name = "footer3", Height = 100, PageBreak = false },
                    SubSection = detail,
                    BreakKey = "break3",
                },
                BreakKey = "",
            },
            BreakKey = "break1",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 4);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Headers[1].Section, subsection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[1].BreakKey, "break1");
        Assert.Equal(sections.Headers[1].BreakCount, 1);
        Assert.Equal(sections.Headers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[2].BreakKey, "");
        Assert.Equal(sections.Headers[2].BreakCount, 1);
        Assert.Equal(sections.Headers[3].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[3].Depth, 3);
        Assert.Equal(sections.Headers[3].BreakKey, "break3");
        Assert.Equal(sections.Headers[3].BreakCount, 2);
        Assert.Equal(sections.Footers.Length, 3);
        Assert.Equal(sections.Footers[0].Section, subsection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[0].Depth, 1);
        Assert.Equal(sections.Footers[0].BreakKey, "break1");
        Assert.Equal(sections.Footers[0].BreakCount, 1);
        Assert.Equal(sections.Footers[1].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[1].Depth, 2);
        Assert.Equal(sections.Footers[1].BreakKey, "");
        Assert.Equal(sections.Footers[1].BreakCount, 1);
        Assert.Equal(sections.Footers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[2].Depth, 3);
        Assert.Equal(sections.Footers[2].BreakKey, "break3");
        Assert.Equal(sections.Footers[2].BreakCount, 2);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break3" });
    }

    [Fact]
    public void GetSectionInfo_Depth3_Sub2NoHeader()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = null,
                Footer = new FooterSection() { Name = "footer2", Height = 100, PageBreak = false },
                SubSection = new Section()
                {
                    Header = new HeaderSection() { Name = "header3", Height = 100 },
                    Footer = new FooterSection() { Name = "footer3", Height = 100, PageBreak = false },
                    SubSection = detail,
                    BreakKey = "break3",
                },
                BreakKey = "break2",
            },
            BreakKey = "break1",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 3);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Headers[1].Section, subsection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[1].BreakKey, "break1");
        Assert.Equal(sections.Headers[1].BreakCount, 1);
        Assert.Equal(sections.Headers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 3);
        Assert.Equal(sections.Headers[2].BreakKey, "break3");
        Assert.Equal(sections.Headers[2].BreakCount, 3);
        Assert.Equal(sections.Footers.Length, 3);
        Assert.Equal(sections.Footers[0].Section, subsection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[0].Depth, 1);
        Assert.Equal(sections.Footers[0].BreakKey, "break1");
        Assert.Equal(sections.Footers[0].BreakCount, 1);
        Assert.Equal(sections.Footers[1].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[1].Depth, 2);
        Assert.Equal(sections.Footers[1].BreakKey, "break2");
        Assert.Equal(sections.Footers[1].BreakCount, 2);
        Assert.Equal(sections.Footers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[2].Depth, 3);
        Assert.Equal(sections.Footers[2].BreakKey, "break3");
        Assert.Equal(sections.Footers[2].BreakCount, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2", "break3" });
    }

    [Fact]
    public void GetSectionInfo_Depth3_Sub2NoFooter()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = new HeaderSection() { Name = "header2", Height = 100 },
                Footer = null,
                SubSection = new Section()
                {
                    Header = new HeaderSection() { Name = "header3", Height = 100 },
                    Footer = new FooterSection() { Name = "footer3", Height = 100, PageBreak = false },
                    SubSection = detail,
                    BreakKey = "break3",
                },
                BreakKey = "break2",
            },
            BreakKey = "break1",
        };
        var sections = SectionBinder.GetSectionInfo(subsection, pageheader);

        Assert.Equal(sections.Headers.Length, 4);
        Assert.Equal(sections.Headers[0].Section, pageheader);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[0].BreakKey, "");
        Assert.Equal(sections.Headers[0].BreakCount, 0);
        Assert.Equal(sections.Headers[1].Section, subsection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[1].BreakKey, "break1");
        Assert.Equal(sections.Headers[1].BreakCount, 1);
        Assert.Equal(sections.Headers[2].Section, subsection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[2].BreakKey, "break2");
        Assert.Equal(sections.Headers[2].BreakCount, 2);
        Assert.Equal(sections.Headers[3].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[3].Depth, 3);
        Assert.Equal(sections.Headers[3].BreakKey, "break3");
        Assert.Equal(sections.Headers[3].BreakCount, 3);
        Assert.Equal(sections.Footers.Length, 2);
        Assert.Equal(sections.Footers[0].Section, subsection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[0].Depth, 1);
        Assert.Equal(sections.Footers[0].BreakKey, "break1");
        Assert.Equal(sections.Footers[0].BreakCount, 1);
        Assert.Equal(sections.Footers[1].Section, subsection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.Footers[1].Depth, 3);
        Assert.Equal(sections.Footers[1].BreakKey, "break3");
        Assert.Equal(sections.Footers[1].BreakCount, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2", "break3" });
    }

    [Fact]
    public void GetSectionInfo_NoDetail()
    {
        var pageheader = new HeaderSection() { Name = "header", Height = 100 };
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        ISubSection subsection = new Section()
        {
            Header = new HeaderSection() { Name = "header1", Height = 100 },
            Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = new HeaderSection() { Name = "header2", Height = 100 },
                Footer = new FooterSection() { Name = "footer2", Height = 100, PageBreak = false },
                SubSection = new Section()
                {
                    Header = new HeaderSection() { Name = "header3", Height = 100 },
                    Footer = new FooterSection() { Name = "footer3", Height = 100, PageBreak = false },
                    SubSection = null!,
                    BreakKey = "break3",
                },
                BreakKey = "break2",
            },
            BreakKey = "break1",
        };

        Assert.Throws<InvalidOperationException>(() => SectionBinder.GetSectionInfo(subsection, pageheader));
    }
}
