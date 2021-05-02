using Sandbox;

namespace PlatformWars
{
    partial class Pawn
    {
        ModelEntity pants;
        ModelEntity jacket;
        ModelEntity shoes;
        ModelEntity hat;

        bool dressed = true;

        static readonly string[] Pants =
        {
            "models/citizen_clothes/trousers/trousers.jeans.vmdl",
            "models/citizen_clothes/dress/dress.kneelength.vmdl",
            "models/citizen/clothes/trousers_tracksuit.vmdl",
            "models/citizen_clothes/shoes/shorts.cargo.vmdl",
            "models/citizen_clothes/trousers/trousers.lab.vmdl"
        };

        static readonly string[] Jackets =
        {
            "models/citizen_clothes/jacket/labcoat.vmdl",
            "models/citizen_clothes/jacket/jacket.red.vmdl",
            "models/citizen_clothes/gloves/gloves_workgloves.vmdl"
        };

        static readonly string[] HairStyles =
        {
            "models/citizen_clothes/hat/hat_hardhat.vmdl",
            "models/citizen_clothes/hat/hat_woolly.vmdl",
            "models/citizen_clothes/hat/hat_securityhelmet.vmdl",
            "models/citizen_clothes/hair/hair_malestyle02.vmdl",
            "models/citizen_clothes/hair/hair_femalebun.black.vmdl"
        };

        /// <summary>
        /// Bit of a hack to put random clothes on the player
        /// </summary>
        public void Dress()
        {
            if (dressed)
                return;

            dressed = true;

            if (Rand.Int(0, 3) != 1)
            {
                var model = Rand.FromArray(Pants);
                pants = new ModelEntity();
                pants.SetModel(model);
                pants.SetParent(this, true);
                pants.EnableShadowInFirstPerson = true;
                pants.EnableHideInFirstPerson = true;

                if (model.Contains("dress"))
                    jacket = pants;
            }

            if (Rand.Int(0, 3) != 1 && jacket == null)
            {
                var model = Rand.FromArray(Jackets);
                jacket = new ModelEntity();
                jacket.SetModel(model);
                jacket.SetParent(this, true);
                jacket.EnableShadowInFirstPerson = true;
                jacket.EnableHideInFirstPerson = true;
            }

            if (Rand.Int(0, 3) != 1)
            {
                shoes = new ModelEntity();
                shoes.SetModel("models/citizen_clothes/shoes/shoes.workboots.vmdl");
                shoes.SetParent(this, true);
                shoes.EnableShadowInFirstPerson = true;
                shoes.EnableHideInFirstPerson = true;
            }

            if (Rand.Int(0, 3) != 1)
            {
                var model = Rand.FromArray(HairStyles);
                hat = new ModelEntity();
                hat.SetModel(model);
                hat.SetParent(this, true);
                hat.EnableShadowInFirstPerson = true;
                hat.EnableHideInFirstPerson = true;
            }
        }
    }
}
