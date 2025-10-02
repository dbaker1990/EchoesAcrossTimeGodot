using Godot;
using System.Text;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Tool to compare different experience curves
    /// </summary>
    public static class ExpCurveComparison
    {
        public static void PrintCurveComparison(int[] levelsToShow = null)
        {
            levelsToShow ??= new int[] { 2, 10, 25, 50, 75, 99, 100 };
            
            var curves = new ExperienceCurve[]
            {
                ExperienceCurve.CreateLinearCurve(),
                ExperienceCurve.CreateExponential1Curve(),
                ExperienceCurve.CreateExponential2Curve(),
                ExperienceCurve.CreateQuadraticCurve(),
                ExperienceCurve.CreateCubicCurve()
            };
            
            var sb = new StringBuilder();
            sb.AppendLine("\n=== EXPERIENCE CURVE COMPARISON ===");
            sb.AppendLine("Level | Linear | Exp1 | Exp2 | Quad | Cubic");
            sb.AppendLine("------|--------|------|------|------|-------");
            
            foreach (int level in levelsToShow)
            {
                sb.Append($"{level,5} |");
                foreach (var curve in curves)
                {
                    int exp = curve.GetExpForLevelUp(level);
                    sb.Append($" {exp,6} |");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("\n=== TOTAL EXP TO REACH LEVEL ===");
            sb.AppendLine("Level | Linear | Exp1 | Exp2 | Quad | Cubic");
            sb.AppendLine("------|--------|------|------|------|-------");
            
            foreach (int level in levelsToShow)
            {
                sb.Append($"{level,5} |");
                foreach (var curve in curves)
                {
                    int totalExp = curve.GetTotalExpForLevel(level);
                    sb.Append($" {totalExp,6} |");
                }
                sb.AppendLine();
            }
            
            GD.Print(sb.ToString());
        }
    }
}