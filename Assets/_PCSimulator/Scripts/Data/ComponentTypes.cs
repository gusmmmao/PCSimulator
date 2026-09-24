namespace PCSimulator.Data
{
    public enum ComponentType
    {
        None,
        CPU,
        Motherboard,
        RAM,
        GPU,
        Storage,
        PSU,
        Case,
        Cooler,
        Cable
    }

    public enum SocketType
    {
        None,
        LGA1700,
        AM4,
        AM5,
        DDR4,
        DDR5,
        PCIe_x16,
        M2_NVMe,
        SATA,
        ATX_24Pin,
        EPS_8Pin,
        PCIe_Power
    }

    public enum InstallationState
    {
        Uninstalled,
        HeldByPlayer,
        Snapping,
        Installed
    }
}
