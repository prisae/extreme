DST=./bin

MATRIX_SRC=Native.Matrix
MPI_SRC=Native.MpiWrapper


PHONY: matrix
PHONY: mpi


include make.inc


all:  mpi matrix

#all: 





matrix: mkdst
ifeq ($(BLAS_TYPE), OPENBLAS)
	$(CC) ${MATRIX_SRC}/*.c $(C_FLAGS)  -D BLAS  -I$(BLAS_INC) -L$(BLAS_LIB) -o ${DST}/libntv_math.so
else
	$(CC) ${MATRIX_SRC}/*.c $(C_FLAGS) -D MKL -mkl=sequential  -o=${DST}/libntv_math.so
endif

mpi:	mkdst
	$(MPICC)  -D $(MPI_TYPE) $(C_FLAGS)  ${MPI_SRC}/*.c -o ${DST}/libntv_mpi.so
mkdst:
	mkdir -p ${DST}
 
.PHONY: all
.PHONY: clean

clean:
	rm -f ${DST}/*.so
