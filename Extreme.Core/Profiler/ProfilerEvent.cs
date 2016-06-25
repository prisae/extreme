//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
﻿using System;

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
		OperatorGiem2gApply,
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
