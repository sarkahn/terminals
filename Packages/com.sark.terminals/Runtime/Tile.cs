using Color = UnityEngine.Color;

namespace Sark.Terminals
{
    [System.Serializable]
    public struct Tile : System.IEquatable<Tile>
    {
        public byte glyph;
        public Color fgColor;
        public Color bgColor;
        
        public static Tile EmptyTile => new Tile
        {
            bgColor = Color.black,
            fgColor = Color.white,
            glyph = 0
        };

        public bool Equals(Tile other)
        {
            return glyph == other.glyph &&
                fgColor.Equals(other.fgColor) &&
                bgColor.Equals(other.bgColor);
        }
    }
}