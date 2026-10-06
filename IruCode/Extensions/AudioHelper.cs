using MegaCrit.Sts2.Core.Commands;

namespace Iru.IruCode.Extensions;

public static class AudioHelper
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] attackSfx =
    {
        "res://Iru/sounds/attack (1).wav",
        "res://Iru/sounds/attack (2).wav",
        "res://Iru/sounds/attack (3).wav",
        "res://Iru/sounds/attack (4).wav",
    };
    
    private static readonly string[] damagedSfx =
    {
        "res://Iru/sounds/hit.wav",
        "res://Iru/sounds/hit_2.wav"
    };
    
    private static readonly string[] highDamagedSfx =
    {
        "res://Iru/sounds/hit_high.wav"
    };
    
    private static readonly string[] criticalDamagedSfx =
    {
        "res://Iru/sounds/hit_critical.wav"
    };
    
    private static readonly string[] defendSfx =
    {
        "res://Iru/sounds/defend (1).wav",
        "res://Iru/sounds/defend (2).wav",
        "res://Iru/sounds/defend (3).wav",
        "res://Iru/sounds/defend (4).wav",
    };
    
    private static readonly string[] victorySfx =
    {
        "res://Iru/sounds/win_1.wav",
        "res://Iru/sounds/win_2.wav",
        "res://Iru/sounds/win_3.wav",
        "res://Iru/sounds/win_4.wav",
        "res://Iru/sounds/win_5.wav",
    };

    private static readonly string[] gameoverSfx =
    {
        "res://Iru/sounds/gameover (1).wav",
        "res://Iru/sounds/gameover (2).wav",
        "res://Iru/sounds/gameover (5).wav",
        "res://Iru/sounds/gameover (4).wav",
        
    };
    
    public static void PlayRandomAttack()
    {
        PlayRandom(attackSfx);
    }
    
    public static void PlayRandomDefend()
    {
        PlayRandom(defendSfx);
    }
    
    public static void PlayRandomDamaged()
    {
        PlayRandom(damagedSfx);
    }

    public static void PlayRandomDamagedHigh()
    {
        PlayRandom(highDamagedSfx);
    }

    public static void PlayRandomGameover()
    {
        PlayRandom(gameoverSfx);
    }

    public static void PlayRandomDamagedCritical()
    {
        PlayRandom(criticalDamagedSfx);
    }
    
    public static void PlayRandomVictory()
    {
        PlayRandom(victorySfx);
    }

    public static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index]);
    }
}