namespace BetterBombs
{
    public class ModConfig
    {
        public bool DamageFarmers { get; set; } = false;
        public float Radius { get; set; } = 1.0f;
        public float Damage { get; set; } = 1.0f;
        public bool BreakClumps { get; set; } = false;
        public bool BreakStoneClumps { get; set; } = false;
        public bool BreakWoodClumps { get; set; } = false;
        public bool BreakWeedsClumps { get; set; } = false;
        public bool BreakOtherClumps { get; set; } = false;
        public bool CollectMinerals { get; set; } = false;
        public bool CollectForage { get; set; } = false;
        public bool CollectFish { get; set; } = false;
        public bool CollectedDegrade { get; set; } = false;
        public bool CollectedQuality { get; set; } = true;
        public bool TillDirt { get; set; } = true;
    }
}
