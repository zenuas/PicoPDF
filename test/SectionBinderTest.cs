using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using System;
using Xunit;

namespace PicoPDF.Test;

public class SectionBinderTest
{
    [Fact]
    public void GetSectionInfo_DetailOnly()
    {
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            SubSection = new DetailSection() { Name = "detail", Height = 100 },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 0);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 0);
        Assert.Equal(sections.Detail, page.SubSection);
        Assert.Equal(sections.BreakKeys.Length, 0);
    }

    [Fact]
    public void GetSectionInfo_Single()
    {
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new DetailSection() { Name = "detail", Height = 100 },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 1);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 0);
        Assert.Equal(sections.Detail, page.SubSection);
        Assert.Equal(sections.BreakKeys.Length, 0);
    }

    [Fact]
    public void GetSectionInfo_Depth1_DetailOnly()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                SubSection = detail,
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 1);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 0);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys.Length, 0);
    }

    [Fact]
    public void GetSectionInfo_Depth1_BreakKyeOnly()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                SubSection = detail,
                BreakKey = "break",
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 1);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 0);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break" });
    }

    [Fact]
    public void GetSectionInfo_Depth1()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
            {
                Header = new HeaderSection() { Name = "header1", Height = 100 },
                Footer = new FooterSection() { Name = "footer1", Height = 100, PageBreak = false },
                SubSection = detail,
                BreakKey = "break",
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 2);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[1].Section, page.SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 1);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Section, page.SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Depth, 1);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break" });
    }

    [Fact]
    public void GetSectionInfo_Depth2()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
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
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 3);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[1].Section, page.SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 2);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Section, page.SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Depth, 1);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Depth, 2);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2" });
    }

    [Fact]
    public void GetSectionInfo_Depth3()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
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
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 4);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[1].Section, page.SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[3].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[3].Depth, 3);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 3);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Section, page.SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Depth, 1);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Depth, 2);
        Assert.Equal(sections.FootersWithoutPageFooter[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[2].Depth, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2", "break3" });
    }

    [Fact]
    public void GetSectionInfo_Depth3_Sub2NoBreak()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
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
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 4);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[1].Section, page.SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[3].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[3].Depth, 3);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 3);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Section, page.SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Depth, 1);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Depth, 2);
        Assert.Equal(sections.FootersWithoutPageFooter[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[2].Depth, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break3" });
    }

    [Fact]
    public void GetSectionInfo_Depth3_Sub2NoHeader()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
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
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 3);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[1].Section, page.SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 3);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 3);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Section, page.SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Depth, 1);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Depth, 2);
        Assert.Equal(sections.FootersWithoutPageFooter[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[2].Depth, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2", "break3" });
    }

    [Fact]
    public void GetSectionInfo_Depth3_Sub2NoFooter()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
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
            },
        };
        var sections = SectionBinder.GetSectionInfo(page);
        Assert.Equal(sections.Headers.Length, 4);
        Assert.Equal(sections.Headers[0].Section, page.Header);
        Assert.Equal(sections.Headers[0].Depth, 0);
        Assert.Equal(sections.Headers[1].Section, page.SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[1].Depth, 1);
        Assert.Equal(sections.Headers[2].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[2].Depth, 2);
        Assert.Equal(sections.Headers[3].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Header);
        Assert.Equal(sections.Headers[3].Depth, 3);
        Assert.Equal(sections.FootersWithoutPageFooter.Length, 2);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Section, page.SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[0].Depth, 1);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Section, page.SubSection.Cast<Section>().SubSection.Cast<Section>().SubSection.Cast<Section>().Footer);
        Assert.Equal(sections.FootersWithoutPageFooter[1].Depth, 3);
        Assert.Equal(sections.Detail, detail);
        Assert.Equal(sections.BreakKeys, new string[] { "break1", "break2", "break3" });
    }

    [Fact]
    public void GetSectionInfo_NoDetail()
    {
        var detail = new DetailSection() { Name = "detail", Height = 100 };
        var page = new PageSection()
        {
            Size = new PageSize(PageSizes.A4),
            Orientation = Orientation.Vertical,
            DefaultFont = ["xxx", "yyy"],
            Header = new HeaderSection() { Name = "header", Height = 100 },
            Footer = new FooterSection() { Name = "footer", Height = 100, PageBreak = false },
            SubSection = new Section()
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
            },
        };
        Assert.Throws<InvalidOperationException>(() => SectionBinder.GetSectionInfo(page));
    }
}
