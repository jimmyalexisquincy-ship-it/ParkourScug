
namespace ParkourScugPlugin.Creatures;

    public static class CreatureTemplateType
    {
        public static CreatureTemplate.Type CamoCentipede = new("CamoCentipede", true); 
        public static CreatureTemplate.Type SmallCamoCentipede = new("SmallCamoCentipede", true);

        public static void UnregisterValues()
        {
            if (CamoCentipede != null)
            {
                CamoCentipede.Unregister();
                CamoCentipede = null;
            }
            if (SmallCamoCentipede != null)
            {
                SmallCamoCentipede.Unregister();
                SmallCamoCentipede = null;
            }

    }
    }

    public static class SandboxUnlockID
    {
        public static MultiplayerUnlocks.SandboxUnlockID CamoCentipede = new("CamoCentipede", true); 
    
        public static MultiplayerUnlocks.SandboxUnlockID SmallCamoCentipede = new("SmallCamoCentipede", true);

        public static void UnregisterValues()
        {
            if (CamoCentipede != null)
            {
                CamoCentipede.Unregister();
                CamoCentipede = null;
            }
            if (SmallCamoCentipede != null)
            {
                SmallCamoCentipede.Unregister();
                SmallCamoCentipede = null;
            }

    }

    }
