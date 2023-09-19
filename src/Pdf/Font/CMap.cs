namespace PicoPDF.Pdf.Font;

public enum CMap
{
    // Chinese (Simplified)
    [CIDSystemInfo("GB-EUC-H", "Adobe", "GB1", 0)]
    GB_EUC_H,

    [CIDSystemInfo("GB-EUC-V", "Adobe", "GB1", 0)]
    GB_EUC_V,

    [CIDSystemInfo("GBpc-EUC-H", "Adobe", "GB1", 0)]
    GBpc_EUC_H,

    [CIDSystemInfo("GBpc-EUC-V", "Adobe", "GB1", 0)]
    GBpc_EUC_V,

    [CIDSystemInfo("GBK-EUC-H", "Adobe", "GB1", 2)]
    GBK_EUC_H,

    [CIDSystemInfo("GBK-EUC-V", "Adobe", "GB1", 2)]
    GBK_EUC_V,

    [CIDSystemInfo("GBKp-EUC-H", "Adobe", "GB1", 2)]
    GBKp_EUC_H,

    [CIDSystemInfo("GBKp-EUC-V", "Adobe", "GB1", 2)]
    GBKp_EUC_V,

    [CIDSystemInfo("GBK2K-H", "Adobe", "GB1", 4)]
    GBK2K_H,

    [CIDSystemInfo("GBK2K-V", "Adobe", "GB1", 4)]
    GBK2K_V,

    [CIDSystemInfo("UniGB-UCS2-H", "Adobe", "GB1", 4)]
    UniGB_UCS2_H,

    [CIDSystemInfo("UniGB-UCS2-V", "Adobe", "GB1", 4)]
    UniGB_UCS2_V,

    [CIDSystemInfo("UniGB-UTF16-H", "Adobe", "GB1", 4)]
    UniGB_UTF16_H,

    [CIDSystemInfo("UniGB-UTF16-V", "Adobe", "GB1", 4)]
    UniGB_UTF16_V,

    // Chinese (Traditional)
    [CIDSystemInfo("B5pc-H", "Adobe", "CNS1", 0)]
    B5pc_H,

    [CIDSystemInfo("B5pc-V", "Adobe", "CNS1", 0)]
    B5pc_V,

    [CIDSystemInfo("HKscs-B5-H", "Adobe", "CNS1", 3)]
    HKscs_B5_H,

    [CIDSystemInfo("HKscs-B5-V", "Adobe", "CNS1", 3)]
    HKscs_B5_V,

    [CIDSystemInfo("ETen-B5-H", "Adobe", "CNS1", 0)]
    ETen_B5_H,

    [CIDSystemInfo("ETen-B5-V", "Adobe", "CNS1", 0)]
    ETen_B5_V,

    [CIDSystemInfo("ETenms-B5-H", "Adobe", "CNS1", 0)]
    ETenms_B5_H,

    [CIDSystemInfo("ETenms-B5-V", "Adobe", "CNS1", 0)]
    ETenms_B5_V,

    [CIDSystemInfo("CNS-EUC-H", "Adobe", "CNS1", 0)]
    CNS_EUC_H,

    [CIDSystemInfo("CNS-EUC-V", "Adobe", "CNS1", 0)]
    CNS_EUC_V,

    [CIDSystemInfo("UniCNS-UCS2-H", "Adobe", "CNS1", 3)]
    UniCNS_UCS2_H,

    [CIDSystemInfo("UniCNS-UCS2-V", "Adobe", "CNS1", 3)]
    UniCNS_UCS2_V,

    [CIDSystemInfo("UniCNS-UTF16-H", "Adobe", "CNS1", 4)]
    UniCNS_UTF16_H,

    [CIDSystemInfo("UniCNS-UTF16-V", "Adobe", "CNS1", 4)]
    UniCNS_UTF16_V,

    // Japanese
    [CIDSystemInfo("83pv-RKSJ-H", "Adobe", "Japan1", 1)]
    _83pv_RKSJ_H,

    [CIDSystemInfo("90ms-RKSJ-H", "Adobe", "Japan1", 2)]
    _90ms_RKSJ_H,

    [CIDSystemInfo("90ms-RKSJ-V", "Adobe", "Japan1", 2)]
    _90ms_RKSJ_V,

    [CIDSystemInfo("90msp-RKSJ-H", "Adobe", "Japan1", 2)]
    _90msp_RKSJ_H,

    [CIDSystemInfo("90msp-RKSJ-V", "Adobe", "Japan1", 2)]
    _90msp_RKSJ_V,

    [CIDSystemInfo("90pv-RKSJ-H", "Adobe", "Japan1", 1)]
    _90pv_RKSJ_H,

    [CIDSystemInfo("Add-RKSJ-H", "Adobe", "Japan1", 1)]
    Add_RKSJ_H,

    [CIDSystemInfo("Add-RKSJ-V", "Adobe", "Japan1", 1)]
    Add_RKSJ_V,

    [CIDSystemInfo("EUC-H", "Adobe", "Japan1", 1)]
    EUC_H,

    [CIDSystemInfo("EUC-V", "Adobe", "Japan1", 1)]
    EUC_V,

    [CIDSystemInfo("Ext-RKSJ-H", "Adobe", "Japan1", 2)]
    Ext_RKSJ_H,

    [CIDSystemInfo("Ext-RKSJ-V", "Adobe", "Japan1", 2)]
    Ext_RKSJ_V,

    [CIDSystemInfo("H", "Adobe", "Japan1", 1)]
    H,

    [CIDSystemInfo("V", "Adobe", "Japan1", 1)]
    V,

    [CIDSystemInfo("UniJIS-UCS2-H", "Adobe", "Japan1", 4)]
    UniJIS_UCS2_H,

    [CIDSystemInfo("UniJIS-UCS2-V", "Adobe", "Japan1", 4)]
    UniJIS_UCS2_V,

    [CIDSystemInfo("UniJIS-UCS2-HW-H", "Adobe", "Japan1", 4)]
    UniJIS_UCS2_HW_H,

    [CIDSystemInfo("UniJIS-UCS2-HW-V", "Adobe", "Japan1", 4)]
    UniJIS_UCS2_HW_V,

    [CIDSystemInfo("UniJIS-UTF16-H", "Adobe", "Japan1", 5)]
    UniJIS_UTF16_H,

    [CIDSystemInfo("UniJIS-UTF16-V", "Adobe", "Japan1", 5)]
    UniJIS_UTF16_V,

    // Korean
    [CIDSystemInfo("KSC-EUC-H", "Adobe", "Korea1", 0)]
    KSC_EUC_H,

    [CIDSystemInfo("KSC-EUC-V", "Adobe", "Korea1", 0)]
    KSC_EUC_V,

    [CIDSystemInfo("KSCms-UHC-H", "Adobe", "Korea1", 1)]
    KSCms_UHC_H,

    [CIDSystemInfo("KSCms-UHC-V", "Adobe", "Korea1", 1)]
    KSCms_UHC_V,

    [CIDSystemInfo("KSCms-UHC-HW-H", "Adobe", "Korea1", 1)]
    KSCms_UHC_HW_H,

    [CIDSystemInfo("KSCms-UHC-HW-V", "Adobe", "Korea1", 1)]
    KSCms_UHC_HW_V,

    [CIDSystemInfo("KSCpc-EUC-H", "Adobe", "Korea1", 0)]
    KSCpc_EUC_H,

    [CIDSystemInfo("UniKS-UCS2-H", "Adobe", "Korea1", 1)]
    UniKS_UCS2_H,

    [CIDSystemInfo("UniKS-UCS2-V", "Adobe", "Korea1", 1)]
    UniKS_UCS2_V,

    [CIDSystemInfo("UniKS-UTF16-H", "Adobe", "Korea1", 2)]
    UniKS_UTF16_H,

    [CIDSystemInfo("UniKS-UTF16-V", "Adobe", "Korea1", 2)]
    UniKS_UTF16_V,

    // Generic
    [CIDSystemInfo("Identity-H", "Adobe", "Identity", 0)]
    Identity_H,

    [CIDSystemInfo("Identity-V", "Adobe", "Identity", 0)]
    Identity_V,
}
