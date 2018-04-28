# General imports
import os
import argparse

# ML imports
from keras import backend as K

# Imports from our code
import models
import keras_to_protobuf
import keras_to_coreml
import chestxray_extract_final_weights

def main(input_model_file, output_directory):
    # Get the filename of the input model file (without the extension) - we'll use this for naming the output files
    input_model_filename = os.path.splitext(os.path.basename(input_model_file))[0]

    # Run all the conversion scripts
    print('')
    print('====================================================================================================================')
    print('Generating TensorFlow Protobuf Model')
    print('====================================================================================================================')
    tensorflow_model_path = os.path.join(output_directory, input_model_filename + '.pb')
    keras_to_protobuf.main('chestxray', input_model_file, tensorflow_model_path)
    K.clear_session()
    
    print('')
    print('====================================================================================================================')
    print('Generating CoreML Model')
    print('====================================================================================================================')
    coreml_model_path = os.path.join(output_directory, input_model_filename + '.mlmodel')
    keras_to_coreml.main('chestxray', input_model_file, coreml_model_path)
    K.clear_session()

    print('')
    print('====================================================================================================================')
    print('Generating CoreML CAM Model')
    print('====================================================================================================================')
    coreml_cam_model_path = os.path.join(output_directory, input_model_filename + '-CAM' + '.mlmodel')
    keras_to_coreml.main('chestxray-cam', input_model_file, coreml_cam_model_path)
    K.clear_session()

    print('')
    print('====================================================================================================================')
    print('Exporting Final Layer Weights')
    print('====================================================================================================================')
    extracted_weights_path = os.path.join(output_directory, input_model_filename + '-FinalLayerWeights' + '.csv')
    chestxray_extract_final_weights.main(input_model_file, extracted_weights_path)
    K.clear_session()
    
    print('')
    print('====================================================================================================================')
    print('Summary')
    print('====================================================================================================================')
    print('    TensorFlow Protobuf Model     : ' + tensorflow_model_path)
    print('    CoreML Model                  : ' + coreml_model_path)
    print('    CoreML CAM Model              : ' + coreml_cam_model_path)
    print('    Extracted Final Layer Weights : ' + extracted_weights_path)
    print('====================================================================================================================')
    print('')

if __name__ == '__main__':
    # Extract arguments
    parser = argparse.ArgumentParser(description='Generates all necessary files (models and weights) to deploy a trained ChestXRay Keras model in the Mobile Chest X-Ray Analysis app')
    parser.add_argument('input_model_file', type=str, action='store', help='path to the saved Keras ChestXRay model')
    parser.add_argument('output_directory', type=str, action='store', help='folder where all generated files will be placed')
    args = parser.parse_args()

    # Call main function
    main(args.input_model_file, args.output_directory)