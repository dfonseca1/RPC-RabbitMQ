namespace Consumer.DataContracts {

    public class CalculateRequest {
        public int ValueX { get; set; }
        public int ValueY { get; set; }
        public EnumOperation Operation { get; set; }
    }
}