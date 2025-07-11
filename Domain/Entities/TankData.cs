﻿namespace Domain.Entities
{
    public class TankData
    {
        public float Volume { get; set; }
        public float TCVolume { get; set; }
        public float? Ullage { get; set; }
        public float Height { get; set; }
        public float Water { get; set; }
        public float Temperature { get; set; }
        public float? WaterVolume { get; set; }
    }

    public class  DeliveryTankData : TankData {
        public DateTime Date { get; set; }
    }
}
