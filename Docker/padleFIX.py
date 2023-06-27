paths = ['/usr/local/lib/python3.10/dist-packages/paddleocr/paddleocr.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/tools/infer/predict_system.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/tools/infer/predict_rec.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/tools/infer/predict_det.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/ppocr/data/imaug/copy_paste.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/tools/infer/predict_cls.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/ppstructure/utility.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/ppstructure/predict_system.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/ppstructure/layout/predict_layout.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/ppstructure/table/predict_table.py',
         '/usr/local/lib/python3.10/dist-packages/paddleocr/ppstructure/table/predict_structure.py']


for path in paths:
    f = open(path, 'r')
    filedata = f.read()
    f.close()
    newdata = filedata.replace("from tools", "from paddleocr.tools")
    newdata = newdata.replace("import tools", "import paddleocr.tools")
    print('rewrite:', path)

    f = open(path, 'w')
    f.write(newdata)
    f.close()
