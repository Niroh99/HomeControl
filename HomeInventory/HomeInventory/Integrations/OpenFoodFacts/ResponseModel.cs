using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HomeInventory.Integrations.OpenFoodFacts
{
    public class ResponseModel
    {
        public class ProductModel
        {
            public class ImageSizeModel
            {
                public Dictionary<string, string> Display { get; set; }

                public Dictionary<string, string> Small { get; set; }

                public Dictionary<string, string> Thumb { get; set; }
            }

            public class SelectedImagesModel
            {
                public ImageSizeModel Front { get; set; }

                public ImageSizeModel Nutrition { get; set; }
            }

            public string Id { get; set; }

            public string Code { get; set; }

            [JsonPropertyName("product_name")]
            public string Name { get; set; }

            [JsonPropertyName("selected_images")]
            public SelectedImagesModel SelectedImages { get; set; }

            public string Brands { get; set; }
        }

        public string Code { get; set; }

        public int Status { get; set; }

        public string SatusVerbose { get; set; }

        public ProductModel Product { get; set; }
    }
}
