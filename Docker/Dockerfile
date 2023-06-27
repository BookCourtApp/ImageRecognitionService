FROM hdgigante/python-opencv:4.7.0-ubuntu


RUN apt update && apt -y upgrade
RUN apt install -y build-essential

#RUN wget https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2004/x86_64/cuda-ubuntu2004.pin
#RUN  mv cuda-ubuntu2004.pin /etc/apt/preferences.d/cuda-repository-pin-600
#RUN wget https://developer.download.nvidia.com/compute/cuda/11.5.0/local_installers/cuda-repo-ubuntu2004-11-5-local_11.5.0-495.29.05-1_amd64.deb
#RUN  dpkg -i cuda-repo-ubuntu2004-11-5-local_11.5.0-495.29.05-1_amd64.deb
#RUN  apt-key add /var/cuda-repo-ubuntu2004-11-5-local/7fa2af80.pub
#RUN  apt-get update
#RUN  apt-get -y install cuda
RUN apt-get install nvidia-cuda-toolkit -y


RUN pip3 install torch torchvision torchaudio
RUN apt-get update -y
RUN apt install git -y

RUN apt-get install gcc -y
RUN pip install cython
RUN apt-get install python3-dev  -y

RUN pip install 'git+https://github.com/facebookresearch/fvcore'
RUN git clone https://github.com/facebookresearch/detectron2.git
RUN apt install ninja-build -y
RUN apt-get install build-essential -y

RUN python3 -m pip install -e detectron2

RUN pip install paddlepaddle
RUN pip install paddleocr

RUN pip install pytesseract

RUN export LD_LIBRARY_PATH=${LD_LIBRARY_PATH}:/usr/local/lib/python3.10/dist-packages

RUN wget http://nz2.archive.ubuntu.com/ubuntu/pool/main/o/openssl/libssl1.1_1.1.1f-1ubuntu2_amd64.deb
RUN dpkg -i libssl1.1_1.1.1f-1ubuntu2_amd64.deb

RUN apt-get install tesseract-ocr-rus

# assign workdir
WORKDIR .
COPY . .

RUN python3 padleFIX.py