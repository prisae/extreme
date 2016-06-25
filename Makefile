
DST=bin

FWD_PATH=./EndUser/ExtremeMt/bin/x64/Release



user: 

	mkdir -p ${DST}
	$(MAKE) -C Native  
	xbuild ExtrEMeMT.sln  /p:Platform="x64" /p:Configuration="Release"


	cp $(FWD_PATH)/*.exe $(DST)
	cp $(FWD_PATH)/*.dll $(DST)
	cp $(FWD_PATH)/*.so $(DST)


clean: 
	$(MAKE) -C Native clean 
	xbuild ExtrEMeMT.sln  /t:Clean /p:Platform="x64" /p:Configuration="Release"

	rm $(FWD_PATH)/*.dll*
	rm $(INV_PATH)/*.dll*
	rm $(FWD_PATH)/*.exe*
	rm $(INV_PATH)/*.exe*
	rm $(FWD2INV_PATH)/*.exe*
	rm $(FWD2INV_PATH)/*.dll* 
