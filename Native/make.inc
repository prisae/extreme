
#PLATFORM=GNU
#PLATFORM=INTEL
#Use make PLATFORM=<> target_name 

ifeq ($(PLATFORM), GNU)

CC=gcc
MPICC=mpicc
C_FLAGS= -O3 -fPIC -shared -std=c99

MPI_TYPE=OpenMPI
BLAS_TYPE=OPENBLAS

BLAS_INC=$(HOME)/lib/OpenBLAS/include
BLAS_LIB=$(HOME)/lib/OpenBLAS/lib/ -lopenblas_seq_i4

endif

ifeq ($(PLATFORM), INTEL)

CC=icc
MPICC=mpicc
C_FLAGS= -O3 -fPIC -shared -std=c99

MPI_TYPE=OpenMPI
BLAS_TYPE=MKL

endif
####################################################################
ifeq ($(PLATFORM), DAINT) #For PizDaint and Cray compiler


CC=cc
MPICC=cc
C_FLAGS= -fPIC  -shared  -O 3 

MPI_TYPE=MPICH
BLAS_TYPE=OPENBLAS

BLAS_INC=$(HOME)/lib/OpenBLAS/seq_i4/include
BLAS_LIB=$(HOME)/lib/OpenBLAS/seq_i4/lib -lopenblas_seq_i4 -lsci_cray

endif


