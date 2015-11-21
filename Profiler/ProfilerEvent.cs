using System;

namespace Extreme.Core
{
    public enum ProfilerEvent
    {
        ForwardSolving,
        ForwardSolvingOneSource,
        CalcJScattered,
        CalcEScattered,
        CalcChi0,
        CalcJq,
        SolveCie,

        GreenAtoATotal,

        GreenScalarAtoACalcCalc,
        GreenScalarAtoAUnion,

        GreenScalarAtoA,
        GreenScalarAtoACalc,
        GreenScalarAtoACommunicate,

        GreenTensorAtoA,
        GreenTensorAtoACalc,
        GreenTensorAtoAFft,
        GreenScalarAtoASegments,
        GreenScalarAtoATrans,

        GreenScalarAtoOForSites,
        GreenTensorAtoOForSites,
        GreenScalarAtoOForLevels,
        GreenTensorAtoOForLevels,

        CalcDotProduct,

        ApplyOperatorA,
        OperatorAMultiplication,
        OperatorAApplyR,

        OperatorAPrepareForForwardFft,
        OperatorAForwardFft,
        OperatorABackwardFft,
        OperatorAExtractAfterBackwardFft,
        OperatorAFinish,

        FftwPlanCalculation,

        ObservationsFullCalculation,
        AtoOGreenCalc,
        AtoOFields,
        SourceFieldCalculation,


        CustomFft,
        CustomFftInitialTranspose,
        CustomFftFinalTranspose,
        CustomFftFourierY,
        CustomFftBlockTransposeYtoX,
        CustomFftDistributedTranspose,
        CustomFftFourierX,
        CustomFftBlockTransposeXtoY,
        CustomFftFourierZ,
    }
}
