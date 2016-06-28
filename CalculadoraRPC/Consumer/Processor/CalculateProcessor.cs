using Consumer.DataContracts;

namespace Consumer.Processor {

    public static class CalculateProcessor {

        public static CalculateResponse Calculate(CalculateRequest request) {
            CalculateResponse response = new CalculateResponse();

            switch (request.Operation) {
                case EnumOperation.Sum:
                response.Result = request.ValueX + request.ValueY;
                break;

                case EnumOperation.Sub:
                response.Result = request.ValueX - request.ValueY;
                break;

                case EnumOperation.Div:
                response.Result = request.ValueX / request.ValueY;
                break;

                case EnumOperation.Mult:
                response.Result = request.ValueX * request.ValueY;
                break;

                default:
                break;
            }

            return response;
        }
    }
}