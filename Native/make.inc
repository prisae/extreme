#Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov

#PLATFORM=GNU
#PLATFORM=INTEL
#Use make PLATFORM=<> target_name 

ifeq ($(PLATFORM), GNU)

CC=gcc
C_FLAGS= -O3 -fPIC -shared -std=c99

BLAS_TYPE=OPENBLAS

BLAS_INC=$(HOME)/lib/OpenBLAS/include
BLAS_LIB=$(HOME)/lib/OpenBLAS/lib/ -lopenblas_seq_i4

ifdef MPI_VERSION
MPI_TYPE=$(MPI_VERSION)
MPICC=mpicc
endif
endif

ifeq ($(PLATFORM), INTEL)

CC=icc
C_FLAGS= -O3 -fPIC -shared -std=c99

BLAS_TYPE=MKL

ifdef MPI_VERSION
MPI_TYPE=$(MPI_VERSION)
MPICC=mpicc
endif
endif
####################################################################


